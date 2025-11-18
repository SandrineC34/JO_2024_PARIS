-- ============================================
-- Script de création des tables JO2024
-- ============================================

USE jo2024_db;

-- ============================================
-- Table Utilisateurs
-- ============================================
CREATE TABLE IF NOT EXISTS Utilisateurs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Prenom VARCHAR(100) NOT NULL,
    Nom VARCHAR(100) NOT NULL,
    Email VARCHAR(255) NOT NULL UNIQUE,
    MotDePasseHash VARCHAR(255) NOT NULL,
    DateCreation DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    DerniereConnexion DATETIME(6) NULL,
    EstActif BOOLEAN NOT NULL DEFAULT TRUE,
    Role VARCHAR(50) NOT NULL DEFAULT 'Utilisateur',
    
    -- Réinitialisation mot de passe
    ResetPasswordToken VARCHAR(255) NULL,
    ResetPasswordExpiry DATETIME(6) NULL,
    
    -- Newsletter
    NewsletterAbonne BOOLEAN NOT NULL DEFAULT FALSE,
    NewsletterCategories TEXT NULL,
    NewsletterSports TEXT NULL,
    NewsletterUnsubscribeToken VARCHAR(255) NULL,
    NewsletterDateInscription DATETIME(6) NULL,
    NewsletterDateDesinscription DATETIME(6) NULL,
    
    INDEX idx_email (Email),
    INDEX idx_reset_token (ResetPasswordToken),
    INDEX idx_newsletter_token (NewsletterUnsubscribeToken)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Table Offres
-- ============================================
CREATE TABLE IF NOT EXISTS Offres (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Nom VARCHAR(200) NOT NULL,
    Description TEXT NOT NULL,
    Prix DECIMAL(10,2) NOT NULL,
    QuantiteDisponible INT NOT NULL,
    DateDebut DATETIME(6) NOT NULL,
    DateFin DATETIME(6) NOT NULL,
    EstActif BOOLEAN NOT NULL DEFAULT TRUE,
    
    INDEX idx_dates (DateDebut, DateFin),
    INDEX idx_actif (EstActif)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Table Commandes
-- ============================================
CREATE TABLE IF NOT EXISTS Commandes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    UtilisateurId INT NOT NULL,
    DateCommande DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    MontantTotal DECIMAL(10,2) NOT NULL,
    Statut VARCHAR(50) NOT NULL DEFAULT 'EnAttente',
    CleSecurity VARCHAR(255) NOT NULL,
    
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE CASCADE,
    INDEX idx_utilisateur (UtilisateurId),
    INDEX idx_date (DateCommande),
    INDEX idx_statut (Statut)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Table Billets
-- ============================================
CREATE TABLE IF NOT EXISTS Billets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CommandeId INT NOT NULL,
    OffreId INT NOT NULL,
    UtilisateurId INT NOT NULL,
    CleSecurity VARCHAR(255) NOT NULL,
    QRCode TEXT NOT NULL,
    DateGeneration DATETIME(6) NOT NULL DEFAULT CURRENT_TIMESTAMP(6),
    EstUtilise BOOLEAN NOT NULL DEFAULT FALSE,
    DateUtilisation DATETIME(6) NULL,
    
    FOREIGN KEY (CommandeId) REFERENCES Commandes(Id) ON DELETE CASCADE,
    FOREIGN KEY (OffreId) REFERENCES Offres(Id) ON DELETE RESTRICT,
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE CASCADE,
    INDEX idx_commande (CommandeId),
    INDEX idx_offre (OffreId),
    INDEX idx_utilisateur (UtilisateurId),
    INDEX idx_qr (QRCode(255)),
    INDEX idx_cle_security (CleSecurity)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Vérification des tables créées
-- ============================================
SHOW TABLES;

-- ============================================
-- Insertion de données de test (optionnel)
-- ============================================

-- Offre de test
INSERT INTO Offres (Nom, Description, Prix, QuantiteDisponible, DateDebut, DateFin, EstActif) VALUES
('Pass Solo', 'Accès à une épreuve de votre choix', 50.00, 1000, '2024-07-26 00:00:00', '2024-08-11 23:59:59', TRUE),
('Pass Duo', 'Accès pour 2 personnes à une épreuve', 90.00, 500, '2024-07-26 00:00:00', '2024-08-11 23:59:59', TRUE),
('Pass Famille', 'Accès pour 4 personnes à une épreuve', 160.00, 300, '2024-07-26 00:00:00', '2024-08-11 23:59:59', TRUE);

SELECT 'Tables créées avec succès !' as Status;