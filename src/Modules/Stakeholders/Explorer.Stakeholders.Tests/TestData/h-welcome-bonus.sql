-- Test data for WelcomeBonus
-- BonusType: AC100=1, AC250=2, AC500=3, Discount10=4, Discount20=5, Discount30=6

-- Tourist -21: AC bonus (already used)
INSERT INTO stakeholders."WelcomeBonuses"("Id", "PersonId", "BonusType", "Value", "IsUsed", "CreatedAt", "ExpiresAt", "UsedAt")
VALUES (-1, -21, 1, 100, true, '2025-01-01 00:00:00', '2025-01-31 00:00:00', '2025-01-15 00:00:00');

-- Tourist -22: Discount bonus (active, not used)
INSERT INTO stakeholders."WelcomeBonuses"("Id", "PersonId", "BonusType", "Value", "IsUsed", "CreatedAt", "ExpiresAt", "UsedAt")
VALUES (-2, -22, 4, 10, false, '2025-01-01 00:00:00', '2026-12-31 00:00:00', NULL);

-- Tourist -23: Discount bonus (already used)
INSERT INTO stakeholders."WelcomeBonuses"("Id", "PersonId", "BonusType", "Value", "IsUsed", "CreatedAt", "ExpiresAt", "UsedAt")
VALUES (-3, -23, 5, 20, true, '2025-01-01 00:00:00', '2025-01-31 00:00:00', '2025-01-10 00:00:00');
