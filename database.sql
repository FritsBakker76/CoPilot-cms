
CREATE DATABASE IF NOT EXISTS cmsmodern CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
USE cmsmodern;

CREATE TABLE users (
    id INT AUTO_INCREMENT PRIMARY KEY,
    username VARCHAR(50) NOT NULL UNIQUE,
    password_hash VARCHAR(255) NOT NULL,
    is_admin BOOLEAN DEFAULT FALSE
);

CREATE TABLE pagecontent (
    id INT AUTO_INCREMENT PRIMARY KEY,
    title VARCHAR(255),
    content TEXT,
    link VARCHAR(500),
    price DECIMAL(18,2),
    duration VARCHAR(100),
    pictureText VARCHAR(500),
    type VARCHAR(100),
    pageId INT,
    position INT,
    created DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (pageId) REFERENCES pages(id)
);

INSERT INTO pages (title, description, content, google_title, google_description) VALUES
('Welcome', 'Welcome to our website', 'Welcome content here.', 'Welcome Page', 'Welcome to our site'),
('Contact', 'Contact us', 'Contact information here.', 'Contact Us', 'Get in touch'),
('News', 'Latest news', 'News content here.', 'News', 'Stay updated');

INSERT INTO pagecontent (title, content, link, price, duration, pictureText, type, pageId, position) VALUES
('Hero Section', 'Welcome to our amazing website!', 'https://example.com', 0.00, 'N/A', 'Hero image', 'hero', 1, 1),
('About Us', 'We are a great company.', NULL, NULL, NULL, NULL, 'text', 1, 2);
