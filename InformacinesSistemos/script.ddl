--@(#) script.ddl

CREATE TABLE Teises
(
	Dokumentas varchar (255),
	GaliojimoData date,
	SuteikimoData date,
	TeisesId int,
	Tipas varchar (255),
	PRIMARY KEY(TeisesId)
);

CREATE TABLE Zanras
(
	Pavadinimas varchar (255),
	ZanroId int,
	PRIMARY KEY(ZanroId)
);

CREATE TABLE Naudotojas
(
	Vardas varchar (255),
	Pavarde varchar (255),
	Slaptažodis varchar (255),
	AsmensKodas int,
	TelefonoNumeris varchar (255),
	Epastas varchar (255),
	Role varchar (255),
	RegistracijosData date,
	PaskyrosBusena varchar (255),
	fk_TeisesTeisesId int NOT NULL,
	PRIMARY KEY(AsmensKodas),
	CONSTRAINT apriboja FOREIGN KEY(fk_TeisesTeisesId) REFERENCES Teises(TeisesId)
);

CREATE TABLE Nusiskundimas
(
	Data date,
	NusiskundimoTekstas varchar (255),
	NusiskundimoId int,
	Statusas varchar (255),
	Tema varchar (255),
	fk_NaudotojasAsmensKodas int NOT NULL,
	PRIMARY KEY(NusiskundimoId),
	CONSTRAINT pateikia FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas)
);

CREATE TABLE Uzsakymas
(
	UzsakymoId int,
	Data date,
	Suma double precision,
	Busena varchar (255),
	PristatymoBudas varchar (255),
	MokejimoTerminasIki date,
	UžsakymoData date,
	fk_NaudotojasAsmensKodas int NOT NULL,
	PRIMARY KEY(UzsakymoId),
	CONSTRAINT kuria FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas)
);

CREATE TABLE Zaidimas
(
	pradžia date,
	ZaidimoId int,
	Kaina double precision,
	Reitingas double precision,
	AmziausCenzas int,
	Kurejas varchar (255),
	ZaidejuSkaicius varchar (255),
	Aprasymas varchar (255),
	fk_NaudotojasAsmensKodas int NOT NULL,
	PRIMARY KEY(ZaidimoId),
	CONSTRAINT parduoda_nuomojaKitiems FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas)
);

CREATE TABLE Apmokejimas
(
	Data date,
	SaskaitosID int,
	ApmokejimoId int,
	MokejimoTipas varchar (255),
	Statusas varchar (255),
	fk_NaudotojasAsmensKodas int NOT NULL,
	fk_UzsakymasUzsakymoId int NOT NULL,
	PRIMARY KEY(SaskaitosID),
	UNIQUE(fk_NaudotojasAsmensKodas),
	UNIQUE(fk_UzsakymasUzsakymoId),
	FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas),
	CONSTRAINT formuoja FOREIGN KEY(fk_UzsakymasUzsakymoId) REFERENCES Uzsakymas(UzsakymoId)
);

CREATE TABLE Atsiliepimas
(
	AtsiliepimoId int,
	Data date,
	AtsiliepimoTeksats varchar (255),
	Ivertinimas int,
	fk_ZaidimasZaidimoId int NOT NULL,
	fk_NaudotojasAsmensKodas int NOT NULL,
	PRIMARY KEY(AtsiliepimoId),
	CONSTRAINT turi FOREIGN KEY(fk_ZaidimasZaidimoId) REFERENCES Zaidimas(ZaidimoId),
	CONSTRAINT raso FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas)
);

CREATE TABLE KompiuteriniaiZaidimai
(
	UzimamaVietaDiske double precision,
	SisteminiaiReikalavimai varchar (255),
	OS varchar (255),
	KompiuterinioZaidimoId int,
	Platforma varchar (255),
	ZaidimoId int,
	PRIMARY KEY(ZaidimoId),
	FOREIGN KEY(ZaidimoId) REFERENCES Zaidimas(ZaidimoId)
);

CREATE TABLE StaloZaidimai
(
	AmziausCenzas int,
	ZaidejuSkaicius varchar (255),
	Ilgis double precision,
	Plotis double precision,
	Aukstis double precision,
	Trukme int,
	Svoris double precision,
	StaloZaidimoId int,
	ZaidimoId int,
	PRIMARY KEY(ZaidimoId),
	FOREIGN KEY(ZaidimoId) REFERENCES Zaidimas(ZaidimoId)
);

CREATE TABLE perziuri
(
	fk_NusiskundimasNusiskundimoId int,
	fk_NaudotojasAsmensKodas int,
	PRIMARY KEY(fk_NusiskundimasNusiskundimoId, fk_NaudotojasAsmensKodas),
	CONSTRAINT perziuri FOREIGN KEY(fk_NusiskundimasNusiskundimoId) REFERENCES Nusiskundimas(NusiskundimoId)
);

CREATE TABLE ZaidimasPriklauso
(
	fk_UzsakymasUzsakymoId int,
	fk_ZaidimasZaidimoId int,
	PRIMARY KEY(fk_UzsakymasUzsakymoId, fk_ZaidimasZaidimoId),
	CONSTRAINT priklauso FOREIGN KEY(fk_UzsakymasUzsakymoId) REFERENCES Uzsakymas(UzsakymoId)
);

CREATE TABLE ZanrasPriklauso
(
	fk_ZaidimasZaidimoId int,
	fk_ZanrasZanroId int,
	PRIMARY KEY(fk_ZaidimasZaidimoId, fk_ZanrasZanroId),
	CONSTRAINT priklauso FOREIGN KEY(fk_ZaidimasZaidimoId) REFERENCES Zaidimas(ZaidimoId)
);

CREATE TABLE Siuntimas
(
	Adresas varchar (255),
	Siuntiniodydis double precision,
	SiunimoId int,
	SiuntimoKaina double precision,
	Busena varchar (255),
	IssiuntimoData date,
	fk_NaudotojasAsmensKodas int NOT NULL,
	fk_ApmokejimasSaskaitosID int NOT NULL,
	fk_UzsakymasUzsakymoId int NOT NULL,
	PRIMARY KEY(SiunimoId),
	UNIQUE(fk_ApmokejimasSaskaitosID),
	UNIQUE(fk_UzsakymasUzsakymoId),
	CONSTRAINT vykdo FOREIGN KEY(fk_NaudotojasAsmensKodas) REFERENCES Naudotojas(AsmensKodas),
	CONSTRAINT priklauso FOREIGN KEY(fk_ApmokejimasSaskaitosID) REFERENCES Apmokejimas(SaskaitosID),
	CONSTRAINT Turi FOREIGN KEY(fk_UzsakymasUzsakymoId) REFERENCES Uzsakymas(UzsakymoId)
);
