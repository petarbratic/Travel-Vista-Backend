-- Ovaj fajl se izvršava POSLE zz-insert-tourist-equipment.sql
-- Dodajemo dodatne test turiste za rank rewards testove

-- Vista (Level 35, XP 3400) - personId -25
INSERT INTO stakeholders."Users"("Id", "Username", "Password", "Role", "IsActive")
SELECT -25, 'turista5@gmail.com', 'turista5', 2, true
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Users" WHERE "Id" = -25);

INSERT INTO stakeholders."People"("Id", "UserId", "Name", "Surname", "Email", "ProfilePictureUrl", "Biography", "Quote")
SELECT -25, -25, 'Nikola', 'Nikolić', 'turista5@gmail.com', null, 'Nikola, Vista rank turista.', 'Nikolin moto.'
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."People" WHERE "Id" = -25);

INSERT INTO stakeholders."Wallets"("Id","PersonId","BalanceAc") 
SELECT -105, -25, 100
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Wallets" WHERE "PersonId" = -25);

INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
SELECT -25, -25, '', 3400, 35
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Tourists" WHERE "PersonId" = -25);

-- Tourist koji je već claim-ovao Gold nagradu - personId -26
INSERT INTO stakeholders."Users"("Id", "Username", "Password", "Role", "IsActive")
SELECT -26, 'turista6@gmail.com', 'turista6', 2, true
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Users" WHERE "Id" = -26);

INSERT INTO stakeholders."People"("Id", "UserId", "Name", "Surname", "Email", "ProfilePictureUrl", "Biography", "Quote")
SELECT -26, -26, 'Marko', 'Marković', 'turista6@gmail.com', null, 'Marko koji je claim-ovao Gold.', 'Markov moto.'
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."People" WHERE "Id" = -26);

INSERT INTO stakeholders."Wallets"("Id","PersonId","BalanceAc") 
SELECT -106, -26, 200
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Wallets" WHERE "PersonId" = -26);

INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
SELECT -26, -26, '', 1200, 13
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Tourists" WHERE "PersonId" = -26);

-- TouristRankRewards - Marko je već claim-ovao Gold
INSERT INTO stakeholders."TouristRankRewards" ("Id", "TouristId", "GoldRewardClaimed", "PlatinumRewardClaimed", "DiamondRewardClaimed", "VistaRewardClaimed")
SELECT -1, -26, true, false, false, false
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."TouristRankRewards" WHERE "TouristId" = -26);

-- NOVI: Dodatni Gold tourist za test tracking update - personId -27
INSERT INTO stakeholders."Users"("Id", "Username", "Password", "Role", "IsActive")
SELECT -27, 'turista7@gmail.com', 'turista7', 2, true
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Users" WHERE "Id" = -27);

INSERT INTO stakeholders."People"("Id", "UserId", "Name", "Surname", "Email", "ProfilePictureUrl", "Biography", "Quote")
SELECT -27, -27, 'Petar', 'Petrović', 'turista7@gmail.com', null, 'Petar za tracking test.', 'Petrov moto.'
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."People" WHERE "Id" = -27);

INSERT INTO stakeholders."Wallets"("Id","PersonId","BalanceAc") 
SELECT -107, -27, 5000
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Wallets" WHERE "PersonId" = -27);

INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
SELECT -27, -27, '', 1100, 12
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Tourists" WHERE "PersonId" = -27);

-- NOVI: Dodatni Diamond tourist za wallet persistence test - personId -28
INSERT INTO stakeholders."Users"("Id", "Username", "Password", "Role", "IsActive")
SELECT -28, 'turista8@gmail.com', 'turista8', 2, true
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Users" WHERE "Id" = -28);

INSERT INTO stakeholders."People"("Id", "UserId", "Name", "Surname", "Email", "ProfilePictureUrl", "Biography", "Quote")
SELECT -28, -28, 'Stefan', 'Stefanović', 'turista8@gmail.com', null, 'Stefan za persistence test.', 'Stefanov moto.'
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."People" WHERE "Id" = -28);

INSERT INTO stakeholders."Wallets"("Id","PersonId","BalanceAc") 
SELECT -108, -28, 7000
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Wallets" WHERE "PersonId" = -28);

INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
SELECT -28, -28, '', 2400, 25
WHERE NOT EXISTS (SELECT 1 FROM stakeholders."Tourists" WHERE "PersonId" = -28);