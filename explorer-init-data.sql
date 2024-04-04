DELETE FROM stakeholders."Users";
DELETE FROM stakeholders."People";

INSERT INTO stakeholders."Users"(
    "Id", "Username", "Password", "Role", "IsActive","VerificationToken")
VALUES (1, 'k', 'glTDKakoUPbVOd03b0gW7idkUX2l4CNVFK9DMWRIDXo=', 1, true,'675da11ebf672f002d23899f66fbab0767dd9a4cbe39dba089fb48e741b673d2');

INSERT INTO stakeholders."People"(
    "Id", "UserId", "Name", "Surname", "Email","ProfileImage","Bio","Quote")
VALUES (1, 1, 'Katarina', 'Perovic' ,'kataleja22@gmail.com', '','','');


