-- Test data for WalletTransactions
-- WalletTransactionType:
-- AdminTopUp = 1
-- CheckoutPurchase = 2
-- WelcomeBonusAc = 3
-- RankRewardAc = 4

-- Tourist -21: Admin top-up (+100 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-201, -21, 100, 1, 'Admin top-up: +100 AC', '2025-01-03 12:00:00', 'AdminTopUp', NULL, -1);

-- Tourist -21: Checkout purchase (-90 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-202, -21, -90, 2, 'Checkout purchase (-90 AC)', '2025-01-04 12:00:00', 'Checkout', 501, NULL);

-- Tourist -21: Welcome bonus (+50 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-203, -21, 50, 3, 'Welcome bonus: +50 AC (X)', '2025-01-05 12:00:00', 'WelcomeBonus', 77, NULL);

-- Tourist -22: Admin top-up (+200 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-204, -22, 200, 1, 'Admin top-up: +200 AC', '2025-01-03 12:00:00', 'AdminTopUp', NULL, -1);

-- Tourist -22: Checkout purchase (-120 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-205, -22, -120, 2, 'Checkout purchase (-120 AC)', '2025-01-04 12:00:00', 'Checkout', 502, NULL);

-- Tourist -23: Rank reward (+30 AC)
INSERT INTO stakeholders."WalletTransactions"
("Id", "PersonId", "AmountAc", "Type", "Description", "CreatedAtUtc", "ReferenceType", "ReferenceId", "InitiatorPersonId")
VALUES
(-206, -23, 30, 4, 'Rank reward: +30 AC (Bronze)', '2025-01-05 12:00:00', 'RankReward', 88, NULL);