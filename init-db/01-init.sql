-- ============================================
-- Script d'initialisation de la base de données
-- JO 2024 Paris - MySQL
-- ============================================

-- Utiliser la base de données
USE jo2024_db;

-- ============================================
-- Création des tables si elles n'existent pas
-- Entity Framework les créera automatiquement,
-- mais ce script sert de fallback
-- ============================================

-- Table Utilisateurs
CREATE TABLE IF NOT EXISTS Utilisateurs (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Email VARCHAR(255) NOT NULL UNIQUE,
    Prenom VARCHAR(100) NOT NULL,
    Nom VARCHAR(100) NOT NULL,
    MotDePasseHash VARCHAR(500) NOT NULL,
    Role VARCHAR(50) NOT NULL DEFAULT 'Utilisateur',
    DateCreation DATETIME DEFAULT CURRENT_TIMESTAMP,
    EstActif BOOLEAN DEFAULT TRUE,
    INDEX idx_email (Email),
    INDEX idx_role (Role)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table Offres
CREATE TABLE IF NOT EXISTS Offres (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Type VARCHAR(50) NOT NULL,
    Nom VARCHAR(200) NOT NULL,
    Description VARCHAR(1000),
    Prix DECIMAL(10,2) NOT NULL,
    NombrePersonnes INT NOT NULL,
    Caracteristiques VARCHAR(500),
    EstActif BOOLEAN DEFAULT TRUE,
    DateCreation DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX idx_type (Type),
    INDEX idx_actif (EstActif)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table Commandes
CREATE TABLE IF NOT EXISTS Commandes (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Numero VARCHAR(50) NOT NULL UNIQUE,
    UtilisateurId INT NOT NULL,
    DateAchat DATETIME DEFAULT CURRENT_TIMESTAMP,
    MontantHT DECIMAL(10,2) NOT NULL,
    MontantTVA DECIMAL(10,2) NOT NULL,
    MontantTotal DECIMAL(10,2) NOT NULL,
    Statut VARCHAR(50) NOT NULL DEFAULT 'Payée',
    MethodePaiement VARCHAR(100),
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE RESTRICT,
    INDEX idx_numero (Numero),
    INDEX idx_utilisateur (UtilisateurId),
    INDEX idx_date (DateAchat)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table CommandeItems
CREATE TABLE IF NOT EXISTS CommandeItems (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    CommandeId INT NOT NULL,
    OffreId INT NOT NULL,
    Quantite INT NOT NULL,
    PrixUnitaire DECIMAL(10,2) NOT NULL,
    PrixTotal DECIMAL(10,2) NOT NULL,
    Sport VARCHAR(100),
    FOREIGN KEY (CommandeId) REFERENCES Commandes(Id) ON DELETE CASCADE,
    FOREIGN KEY (OffreId) REFERENCES Offres(Id) ON DELETE RESTRICT,
    INDEX idx_commande (CommandeId),
    INDEX idx_offre (OffreId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Table Billets
CREATE TABLE IF NOT EXISTS Billets (
    Id INT AUTO_INCREMENT PRIMARY KEY,
    Numero VARCHAR(50) NOT NULL UNIQUE,
    Titre VARCHAR(200) NOT NULL,
    Sport VARCHAR(100) NOT NULL,
    Lieu VARCHAR(200) NOT NULL,
    DateEpreuve DATETIME NOT NULL,
    Place VARCHAR(50),
    Statut VARCHAR(50) NOT NULL DEFAULT 'Actif',
    CodeQR VARCHAR(500) NOT NULL,
    CommandeId INT NOT NULL,
    UtilisateurId INT NOT NULL,
    DateCreation DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CommandeId) REFERENCES Commandes(Id) ON DELETE CASCADE,
    FOREIGN KEY (UtilisateurId) REFERENCES Utilisateurs(Id) ON DELETE RESTRICT,
    INDEX idx_numero (Numero),
    INDEX idx_commande (CommandeId),
    INDEX idx_utilisateur (UtilisateurId),
    INDEX idx_date_epreuve (DateEpreuve)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================
-- Données de test (optionnel)
-- ============================================

-- Insérer des offres de test
INSERT INTO Offres (Type, Nom, Description, Prix, NombrePersonnes, Caracteristiques, EstActif) VALUES
('Solo', 'Pass Solo', 'Billet individuel pour une épreuve', 50.00, 1, 'Accès à une épreuve', TRUE),
('Duo', 'Pass Duo', 'Billet pour 2 personnes', 90.00, 2, 'Accès pour 2 personnes', TRUE),
('Famille', 'Pass Famille', 'Billet famille (4 personnes)', 160.00, 4, 'Accès pour 4 personnes', TRUE)
ON DUPLICATE KEY UPDATE Type=Type;

-- Message de confirmation
SELECT '✅ Base de données initialisée avec succès!' AS Message;
