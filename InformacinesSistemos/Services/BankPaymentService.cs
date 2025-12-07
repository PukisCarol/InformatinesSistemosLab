
using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Npgsql;
using InformacinesSistemos.Models;

namespace InformacinesSistemos.Services
{
    public class BankPaymentService : IBankPaymentService
    {
        private readonly string _connString;

        public BankPaymentService(IConfiguration cfg)
        {
            _connString = cfg.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("DefaultConnection nerastas.");
        }

        // Palieku be pakeitimų tavo kortelės nurašymo logiką (naudoja lentelę 'bankas')
        public async Task<(bool ok, string message)> TryChargeAsync(CardPaymentForm card, decimal amount)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();
            await using var tx = await conn.BeginTransactionAsync(IsolationLevel.Serializable);

            try
            {
                const string sqlSelect = @"
                SELECT saskaitoje_likutis
                FROM public.bankas
                WHERE kortelesid = @id
                  AND vardas = @v
                  AND pavarde = @p
                  AND pinkodas = @pin
                FOR UPDATE";

                await using var selectCmd = new NpgsqlCommand(sqlSelect, conn, tx);
                selectCmd.Parameters.AddWithValue("@id", card.KortelesID);
                selectCmd.Parameters.AddWithValue("@v", card.Vardas);
                selectCmd.Parameters.AddWithValue("@p", card.Pavarde);
                selectCmd.Parameters.AddWithValue("@pin", card.PinKodas);

                var obj = await selectCmd.ExecuteScalarAsync();
                if (obj == null)
                {
                    await tx.RollbackAsync();
                    return (false, "Nesėkmingas mokėjimas: kortelės duomenys neteisingi.");
                }

                var likutis = Convert.ToDecimal(obj);
                if (likutis < amount)
                {
                    await tx.RollbackAsync();
                    return (false, "Nesėkmingas mokėjimas: sąskaitoje nepakanka lėšų.");
                }

                const string sqlUpdate = @"
                UPDATE public.bankas
                SET saskaitoje_likutis = saskaitoje_likutis - @amt
                WHERE kortelesid = @id";

                await using var updateCmd = new NpgsqlCommand(sqlUpdate, conn, tx);
                updateCmd.Parameters.AddWithValue("@amt", amount);
                updateCmd.Parameters.AddWithValue("@id", card.KortelesID);

                var affected = await updateCmd.ExecuteNonQueryAsync();
                if (affected != 1)
                {
                    await tx.RollbackAsync();
                    return (false, "Nesėkmingas mokėjimas: nepavyko nurašyti lėšų.");
                }

                await tx.CommitAsync();
                return (true, "Sėkmingas mokėjimas.");
            }
            catch (Exception ex)
            {
                try { await tx.RollbackAsync(); } catch { /* ignore */ }
                return (false, $"Nesėkmingas mokėjimas: vidinė klaida ({ex.Message}).");
            }
        }

        // Pagal tavo DDL įrašome: 1) Užsakymas; 2) Apmokėjimas; 3) Siuntimas.
        // Viskas vienoje transakcijoje, kad FK visada galiotų.
        public async Task<int?> InsertShippingAsync(ShippingRecord s)
        {
            await using var conn = new NpgsqlConnection(_connString);
            await conn.OpenAsync();

            await using var tx = await conn.BeginTransactionAsync(IsolationLevel.Serializable);
            try
            {
                // 0) Privalomas AK
                if (s.FkNaudotojasAsmensKodas == null)
                {
                    await tx.RollbackAsync();
                    return null;
                }

                // 0.1) Patikriname, ar toks naudotojas egzistuoja
                const string sqlCheckUser = @"SELECT 1 FROM ""Naudotojas"" WHERE ""AsmensKodas"" = @ak LIMIT 1";
                await using (var checkUserCmd = new NpgsqlCommand(sqlCheckUser, conn, tx))
                {
                    checkUserCmd.Parameters.AddWithValue("@ak", s.FkNaudotojasAsmensKodas.Value);
                    var exists = await checkUserCmd.ExecuteScalarAsync();
                    if (exists == null)
                    {
                        await tx.RollbackAsync();
                        return null; // Naudotojas nerastas
                    }
                }

                // 1) Užsakymas: jei paduotas – naudojame, jei ne – sukuriame naują (nes pagal tavo verslo logiką pirkimas formuoja užsakymą dabar)
                int uzsakymoId;
                if (s.FkUzsakymasSuUzsakymoId is int providedOrderId)
                {
                    uzsakymoId = providedOrderId;
                }
                else
                {
                    // Kadangi DDL nenumato identity, parenkame naują ID = MAX+1
                    const string sqlNextOrderId = @"SELECT COALESCE(MAX(""UzsakymoId""), 0) + 1 FROM ""Uzsakymas""";
                    await using var nextOrderCmd = new NpgsqlCommand(sqlNextOrderId, conn, tx);
                    uzsakymoId = Convert.ToInt32(await nextOrderCmd.ExecuteScalarAsync());

                    const string sqlInsertOrder = @"
                        INSERT INTO ""Uzsakymas""
                          (""UzsakymoId"", ""Data"", ""Suma"", ""Busena"", ""PristatymoBudas"", ""MokejimoTerminasIki"", ""UžsakymoData"", ""fk_NaudotojasAsmensKodas"")
                        VALUES
                          (@id, CURRENT_DATE, @suma, @busena, @pristatymo, CURRENT_DATE, CURRENT_DATE, @ak)";
                    await using var insertOrderCmd = new NpgsqlCommand(sqlInsertOrder, conn, tx);
                    insertOrderCmd.Parameters.AddWithValue("@id", uzsakymoId);
                    // Idealiai čia turėtų būti visa krepšelio suma; jei jos neturi šiame metode, laikinai panaudoju siuntimo kainą
                    insertOrderCmd.Parameters.AddWithValue("@suma", s.SiuntimoKaina);
                    insertOrderCmd.Parameters.AddWithValue("@busena", "Sukurta");        // pritaikyk, jei reikia
                    insertOrderCmd.Parameters.AddWithValue("@pristatymo", "Kurjeris");   // pritaikyk, jei reikia
                    insertOrderCmd.Parameters.AddWithValue("@ak", s.FkNaudotojasAsmensKodas.Value);
                    await insertOrderCmd.ExecuteNonQueryAsync();
                }

                // 2) Apmokėjimas: jei šiam užsakymui jau yra įrašas — panaudojame; jei nėra — sukuriame
                int saskaitosId;
                {
                    const string sqlFindPayForOrder = @"
                        SELECT ""SaskaitosID""
                        FROM ""Apmokejimas""
                        WHERE ""fk_UzsakymasUzsakymoId"" = @uzsakymoId
                        LIMIT 1";
                    await using var findPayCmd = new NpgsqlCommand(sqlFindPayForOrder, conn, tx);
                    findPayCmd.Parameters.AddWithValue("@uzsakymoId", uzsakymoId);
                    var payObj = await findPayCmd.ExecuteScalarAsync();

                    if (payObj != null)
                    {
                        saskaitosId = Convert.ToInt32(payObj);
                    }
                    else
                    {
                        const string sqlNextAccountId = @"SELECT COALESCE(MAX(""SaskaitosID""), 0) + 1 FROM ""Apmokejimas""";
                        await using var nextAccCmd = new NpgsqlCommand(sqlNextAccountId, conn, tx);
                        saskaitosId = Convert.ToInt32(await nextAccCmd.ExecuteScalarAsync());

                        const string sqlInsertPayment = @"
                            INSERT INTO ""Apmokejimas""
                              (""Data"", ""SaskaitosID"", ""ApmokejimoId"", ""MokejimoTipas"", ""Statusas"",
                               ""fk_NaudotojasAsmensKodas"", ""fk_UzsakymasUzsakymoId"")
                            VALUES
                              (CURRENT_DATE, @saskId, @payId, @tipas, @statusas, @ak, @uzsakymoId)";
                        await using var insertPayCmd = new NpgsqlCommand(sqlInsertPayment, conn, tx);
                        insertPayCmd.Parameters.AddWithValue("@saskId", saskaitosId);
                        // Jei nori atskiro ApmokejimoId — generuok taip pat MAX+1. Dabar prilyginu SaskaitosID.
                        insertPayCmd.Parameters.AddWithValue("@payId", saskaitosId);
                        insertPayCmd.Parameters.AddWithValue("@tipas", "Kortele");       // pritaikyk, jei reikia
                        insertPayCmd.Parameters.AddWithValue("@statusas", "Apmokėtas");   // pritaikyk, jei reikia
                        insertPayCmd.Parameters.AddWithValue("@ak", s.FkNaudotojasAsmensKodas.Value);
                        insertPayCmd.Parameters.AddWithValue("@uzsakymoId", uzsakymoId);
                        await insertPayCmd.ExecuteNonQueryAsync();
                    }
                }

                // 3) Įrašome siuntimą (SiunimoId irgi ranka: MAX+1)
                const string sqlNextShipId = @"SELECT COALESCE(MAX(""SiunimoId""), 0) + 1 FROM ""Siuntimas""";
                await using var nextShipCmd = new NpgsqlCommand(sqlNextShipId, conn, tx);
                var siunimoId = Convert.ToInt32(await nextShipCmd.ExecuteScalarAsync());

                const string sqlInsertShipping = @"
                    INSERT INTO ""Siuntimas""
                      (""Adresas"", ""Siuntiniodydis"", ""SiunimoId"", ""SiuntimoKaina"", ""Busena"", ""IssiuntimoData"",
                       ""fk_NaudotojasAsmensKodas"", ""fk_ApmokejimasSaskaitosID"", ""fk_UzsakymasUzsakymoId"")
                    VALUES
                      (@adresas, @dydis, @siunimoId, @kaina, @busena, @data,
                       @ak, @saskaitosId, @uzsakymoId)
                    RETURNING ""SiunimoId""";

                await using var cmd = new NpgsqlCommand(sqlInsertShipping, conn, tx);
                cmd.Parameters.AddWithValue("@adresas", s.Adresas);
                cmd.Parameters.AddWithValue("@dydis", 0.0); // jei nenaudoji – palik 0
                cmd.Parameters.AddWithValue("@siunimoId", siunimoId);
                cmd.Parameters.AddWithValue("@kaina", s.SiuntimoKaina);
                cmd.Parameters.AddWithValue("@busena", s.Busena ?? "Paruoštas");

                var shipDate = s.IsSiuntimoData?.ToDateTime(new TimeOnly(0, 0)) ?? DateTime.Today;
                cmd.Parameters.AddWithValue("@data", shipDate);

                cmd.Parameters.AddWithValue("@ak", s.FkNaudotojasAsmensKodas.Value);
                // Naudojame ką tik rastą/sukurtą apmokėjimą ir užsakymą
                // Jei nori naudoti jau esantį s.FkApmokejimasSekaltosId — čia gali pakeisti, bet DDL reikalauja, kad tas ID egzistuotų 'Apmokejimas' lentelėje.
                cmd.Parameters.AddWithValue("@saskaitosId", saskaitosId);
                cmd.Parameters.AddWithValue("@uzsakymoId", uzsakymoId);

                var idObj = await cmd.ExecuteScalarAsync();
                if (idObj == null)
                {
                    await tx.RollbackAsync();
                    return null;
                }

                await tx.CommitAsync();
                return Convert.ToInt32(idObj);
            }
            catch (Exception)
            {
                try { await tx.RollbackAsync(); } catch { /* ignore */ }
                return null;
            }
        }
    }
}
