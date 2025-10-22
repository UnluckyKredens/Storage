# CATEGORY

USE storage;
INSERT INTO category (name)
VALUES 
  ('Electronics'),
  ('Furniture');


# ITEM

INSERT INTO item (name, sku, categoryId, bruttoPrice, nettoPrice)
VALUES
  ('Laptop Pro 15',          'LTP-001', 1, 5500, 4471),
  ('Smartphone X200',        'PHN-002', 1, 3200, 2601),
  ('Bluetooth Headphones',   'AUD-003', 1, 450,  366),
  ('Microwave Oven',         'KIT-004', 1, 850,  691),
  ('Vacuum Cleaner Turbo',   'KIT-005', 1, 1200, 975),
  ('Office Desk',            'FUR-006', 2, 950,  772),
  ('Ergonomic Chair',        'FUR-007', 2, 780,  633),
  ('Bookshelf Classic',      'FUR-008', 2, 620,  504),
  ('Football Ball Pro',      'SPT-009', 2, 220,  179),
  ('Tennis Racket Elite',    'SPT-010', 2, 460,  374);



# STOCK

INSERT INTO stock (itemId, quantity)
VALUES
  (1, 15),
  (2, 25),
  (3, 0),   -- out of stock
  (4, 12),
  (5, 6),
  (6, 10),
  (7, 8),
  (8, 0),   -- out of stock
  (9, 30),
  (10, 5);
