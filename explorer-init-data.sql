DELETE FROM stakeholders."Users";
DELETE FROM stakeholders."People";

/*sifra svuda k */
INSERT INTO stakeholders."Users"(
    "Id", "Username", "Password", "Role", "IsActive","VerificationToken")
VALUES (1, 'admin', 'glTDKakoUPbVOd03b0gW7idkUX2l4CNVFK9DMWRIDXo=', 0, true,'675da11ebf672f002d23899f66fbab0767dd9a4cbe39dba089fb48e741b673d2');
INSERT INTO stakeholders."Users"(
    "Id", "Username", "Password", "Role", "IsActive","VerificationToken")
VALUES (2, 'autor', 'glTDKakoUPbVOd03b0gW7idkUX2l4CNVFK9DMWRIDXo=', 1, true,'675da11ebf672f002d23899f66fbab0767dd9a4cbe39dba089fb48e741b673d2');
INSERT INTO stakeholders."Users"(
    "Id", "Username", "Password", "Role", "IsActive","VerificationToken")
VALUES (3, 'turista', 'glTDKakoUPbVOd03b0gW7idkUX2l4CNVFK9DMWRIDXo=', 2, true,'675da11ebf672f002d23899f66fbab0767dd9a4cbe39dba089fb48e741b673d2');

INSERT INTO stakeholders."People"(
    "Id", "UserId", "Name", "Surname", "Email","ProfileImage","Bio","Quote")
VALUES (1, 1, 'Katarina', 'Njezic' ,'kataleja22@gmail.com', '','','');
INSERT INTO stakeholders."People"(
    "Id", "UserId", "Name", "Surname", "Email","ProfileImage","Bio","Quote")
VALUES (2, 2, 'Nikolina', 'Maric' ,'kataleja22@gmail.com', '','','');
INSERT INTO stakeholders."People"(
    "Id", "UserId", "Name", "Surname", "Email","ProfileImage","Bio","Quote")
VALUES (3, 3, 'Maja', 'Manic' ,'kataleja22@gmail.com', '','','');


