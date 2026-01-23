DELETE FROM stakeholders."Tourists";

-- Tourist -21: Bronze rank (Level 3, XP 200) - može pristupiti meetups-ima
INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
VALUES (-21, -21, '-1,-2', 200, 3);

-- Tourist -22: Diamond rank (Level 25, XP 2400) - za test multiple rewards at once
INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
VALUES (-22, -22, '', 2400, 25);

-- Tourist -23: Gold rank (Level 12, XP 1100) - za test single Gold reward
INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
VALUES (-23, -23, '', 1100, 12);

-- Tourist -24: Rookie rank (Level 1, XP 0) - NE može pristupiti meetups-ima
INSERT INTO stakeholders."Tourists" ("Id", "PersonId", "EquipmentIds", "XP", "Level")
VALUES (-24, -24, '', 0, 1);