// compte.js - Gestion de la page Mon Compte avec appels API

// Configuration de l'API
const API_BASE_URL = '/api';

// État de l'application
let currentUser = null;
let userTickets = [];
let userOrders = [];

// ===========================
// INITIALISATION
// ===========================

document.addEventListener('DOMContentLoaded', async () => {
    console.log('Initialisation de la page Mon Compte');
    
    // Vérifier l'authentification
    await checkAuthentication();
    
    // Charger les données initiales
    await loadUserData();
    
    // Configurer la navigation
    setupNavigation();
    
    // Afficher la section par défaut
    showSection('tickets');
});

// ===========================
// AUTHENTIFICATION
// ===========================

async function checkAuthentication() {
    try {
        const response = await fetch(`${API_BASE_URL}/Auth/current`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            // Rediriger vers la page de connexion
            window.location.href = '/connexion.html';
            return;
        }
        
        currentUser = await response.json();
        updateWelcomeMessage();
    } catch (error) {
        console.error('Erreur d\'authentification:', error);
        showError('Erreur de connexion. Veuillez vous reconnecter.');
        // Redirection après 2 secondes
        setTimeout(() => {
            window.location.href = '/connexion.html';
        }, 2000);
    }
}

function updateWelcomeMessage() {
    const welcomeElement = document.getElementById('welcomeMessage');
    if (welcomeElement && currentUser) {
        welcomeElement.textContent = `Bienvenue ${currentUser.prenom} ${currentUser.nom}`;
    }
}

// ===========================
// CHARGEMENT DES DONNÉES
// ===========================

async function loadUserData() {
    try {
        // Charger en parallèle pour plus de rapidité
        await Promise.all([
            loadTickets(),
            loadOrders(),
            loadUserProfile()
        ]);
    } catch (error) {
        console.error('Erreur de chargement des données:', error);
        showError('Impossible de charger vos données');
    }
}

// Charger les billets
async function loadTickets() {
    const loader = document.getElementById('ticketsLoader');
    const container = document.getElementById('ticketsContainer');
    
    try {
        // Afficher le loader
        if (loader) loader.style.display = 'block';
        
        const response = await fetch(`${API_BASE_URL}/Billets`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors du chargement des billets');
        }
        
        userTickets = await response.json();
        displayTickets();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de charger vos billets');
        userTickets = [];
        displayTickets();
    } finally {
        // Cacher le loader
        if (loader) loader.style.display = 'none';
    }
}

function displayTickets() {
    const container = document.getElementById('ticketsContainer');
    
    if (!container) return;
    
    // Cas : Aucun billet
    if (userTickets.length === 0) {
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: #666;">
                <p style="font-size: 1.2em;">📭 Vous n'avez pas encore de billets</p>
                <p style="margin-top: 10px;">Découvrez nos offres et réservez vos places pour les JO 2024 !</p>
                <a href="/offres.html" class="btn btn-primary" style="margin-top: 20px; display: inline-block;">
                    🎫 Découvrir les offres
                </a>
            </div>
        `;
        return;
    }
    
    // Afficher les billets
    container.innerHTML = userTickets.map(billet => `
        <div class="ticket-card" onclick="toggleTicketDetails(this)">
            <div class="ticket-header">
                <div class="ticket-title">${escapeHtml(billet.titre)}</div>
                <div class="ticket-status ${getStatusClass(billet.statut)}">
                    ${escapeHtml(billet.statut)}
                </div>
            </div>
            <div class="ticket-info">
                📅 ${formatDate(billet.dateEpreuve)}<br>
                📍 ${escapeHtml(billet.lieu)}<br>
                🎫 Billet #${escapeHtml(billet.numero)}
            </div>
            <div class="ticket-details">
                <div class="detail-row">
                    <span><strong>Numéro du billet :</strong></span>
                    <span>${escapeHtml(billet.numero)}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Statut :</strong></span>
                    <span>${getStatusText(billet)}</span>
                </div>
                <div class="detail-row">
                    <span><strong>Place :</strong></span>
                    <span>${escapeHtml(billet.place || 'Non attribuée')}</span>
                </div>
                <div class="qr-actions">
                    ${billet.statut === 'Actif' ? `
                        <button class="btn btn-primary btn-small" onclick="viewQRCode(event, ${billet.id})">
                            👁️ Voir le QR code
                        </button>
                        <button class="btn btn-secondary btn-small" onclick="downloadPDF(event, ${billet.id})">
                            📄 Télécharger PDF
                        </button>
                        <button class="btn btn-outline btn-small" onclick="sendByEmail(event, ${billet.id})">
                            📧 Envoyer par email
                        </button>
                    ` : `
                        <button class="btn btn-secondary btn-small" disabled>
                            ✅ Billet utilisé
                        </button>
                        <button class="btn btn-secondary btn-small" onclick="downloadPDF(event, ${billet.id})">
                            📄 Télécharger PDF
                        </button>
                    `}
                </div>
            </div>
        </div>
    `).join('');
}

// Charger les commandes
async function loadOrders() {
    const loader = document.getElementById('ordersLoader');
    const tbody = document.getElementById('ordersContainer');
    
    try {
        // Afficher le loader
        if (loader) loader.style.display = 'block';
        
        const response = await fetch(`${API_BASE_URL}/Commandes`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors du chargement des commandes');
        }
        
        userOrders = await response.json();
        displayOrders();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de charger vos commandes');
        userOrders = [];
        displayOrders();
    } finally {
        // Cacher le loader
        if (loader) loader.style.display = 'none';
    }
}

function displayOrders() {
    const tbody = document.getElementById('ordersContainer');
    
    if (!tbody) return;
    
    // Cas : Aucune commande
    if (userOrders.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; padding: 40px; color: #666;">
                    📭 Aucune commande pour le moment<br>
                    <small style="margin-top: 10px; display: block;">
                        Vos commandes apparaîtront ici après votre premier achat
                    </small>
                </td>
            </tr>
        `;
        return;
    }
    
    // Afficher les commandes
    tbody.innerHTML = userOrders.map(commande => `
        <tr>
            <td><strong>${escapeHtml(commande.numero)}</strong></td>
            <td>
                ${formatDate(commande.dateAchat)}<br>
                <small>${formatTime(commande.dateAchat)}</small>
            </td>
            <td>
                ${commande.items && commande.items.length > 0 ? 
                    commande.items.map(item => `
                        • ${escapeHtml(item.offreNom)}<br>
                        <small>${item.quantite}x billet${item.quantite > 1 ? 's' : ''} à ${item.prix.toFixed(2)}€</small>
                    `).join('<br>') 
                    : 'Détails non disponibles'
                }
            </td>
            <td><strong>${commande.montantTotal.toFixed(2)}€</strong></td>
            <td>
                <span class="ticket-status ${getStatusClass(commande.statut)}">
                    ${escapeHtml(commande.statut)}
                </span>
            </td>
        </tr>
    `).join('');
}

// Charger le profil utilisateur
async function loadUserProfile() {
    const loader = document.getElementById('settingsLoader');
    const form = document.getElementById('profileForm');
    
    try {
        // Afficher le loader
        if (loader) loader.style.display = 'block';
        
        const response = await fetch(`${API_BASE_URL}/Compte/profile`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors du chargement du profil');
        }
        
        const profile = await response.json();
        fillProfileForm(profile);
        
        // Afficher le formulaire
        if (form) form.style.display = 'block';
    } catch (error) {
        console.error('Erreur:', error);
        // En cas d'erreur, utiliser les données de currentUser si disponibles
        if (currentUser) {
            fillProfileForm({
                prenom: currentUser.prenom,
                nom: currentUser.nom,
                email: currentUser.email || ''
            });
            if (form) form.style.display = 'block';
        }
    } finally {
        // Cacher le loader
        if (loader) loader.style.display = 'none';
    }
}

function fillProfileForm(profile) {
    document.getElementById('firstName').value = profile.prenom || '';
    document.getElementById('lastName').value = profile.nom || '';
    document.getElementById('email').value = profile.email || '';
}

// ===========================
// ACTIONS SUR LES BILLETS
// ===========================

function toggleTicketDetails(card) {
    const details = card.querySelector('.ticket-details');
    const isOpen = details.style.display === 'block';
    
    // Fermer tous les détails ouverts
    document.querySelectorAll('.ticket-details').forEach(d => {
        d.style.display = 'none';
    });
    
    // Ouvrir celui-ci si il était fermé
    if (!isOpen) {
        details.style.display = 'block';
    }
}

async function viewQRCode(event, billetId) {
    event.stopPropagation();
    
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de la récupération du QR code');
        }
        
        const billet = await response.json();
        showQRCodeModal(billet);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible d\'afficher le QR code');
    }
}

function showQRCodeModal(billet) {
    // Créer la modal
    const modal = document.createElement('div');
    modal.className = 'modal-overlay';
    modal.innerHTML = `
        <div class="modal-content">
            <div class="modal-header">
                <h3>QR Code - ${escapeHtml(billet.numero)}</h3>
                <button class="modal-close" onclick="this.closest('.modal-overlay').remove()">
                    ✕
                </button>
            </div>
            <div class="modal-body" style="text-align: center; padding: 30px;">
                <img src="${escapeHtml(billet.codeQR)}" alt="QR Code" style="max-width: 300px; border: 2px solid #004e92;">
                <p style="margin-top: 20px; color: #666;">
                    ${escapeHtml(billet.titre)}<br>
                    📅 ${formatDate(billet.dateEpreuve)}
                </p>
                <p style="margin-top: 10px; font-size: 0.9em; color: #999;">
                    Présentez ce QR code à l'entrée de l'événement
                </p>
            </div>
        </div>
    `;
    
    document.body.appendChild(modal);
    
    // Fermer en cliquant en dehors
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.remove();
        }
    });
}

async function downloadPDF(event, billetId) {
    event.stopPropagation();
    
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}/download`, {
            method: 'POST',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors du téléchargement');
        }
        
        const result = await response.json();
        showSuccess('PDF généré avec succès ! Téléchargement en cours...');
        
        // TODO: Implémenter le téléchargement réel du PDF
        console.log('Download URL:', result.downloadUrl);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de télécharger le PDF');
    }
}

async function sendByEmail(event, billetId) {
    event.stopPropagation();
    
    if (!confirm('Voulez-vous recevoir ce billet par email ?')) {
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/Billets/${billetId}/email`, {
            method: 'POST',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de l\'envoi');
        }
        
        const result = await response.json();
        showSuccess(result.message);
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible d\'envoyer l\'email');
    }
}

// ===========================
// GESTION DU PROFIL
// ===========================

async function saveSettings(event) {
    event.preventDefault();
    
    const formData = {
        prenom: document.getElementById('firstName').value,
        nom: document.getElementById('lastName').value,
        email: document.getElementById('email').value
    };
    
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/profile`, {
            method: 'PUT',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify(formData)
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de la mise à jour');
        }
        
        showSuccess('✅ Informations mises à jour avec succès !');
        
        // Recharger les données utilisateur
        await checkAuthentication();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de mettre à jour vos informations');
    }
}

async function changePassword(event) {
    event.preventDefault();
    
    const currentPassword = document.getElementById('currentPassword').value;
    const newPassword = document.getElementById('newPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;
    
    // Validation
    if (newPassword !== confirmPassword) {
        showError('Les mots de passe ne correspondent pas');
        return;
    }
    
    if (!validatePassword(newPassword)) {
        showError('Le mot de passe ne respecte pas les critères de sécurité');
        return;
    }
    
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/change-password`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            credentials: 'include',
            body: JSON.stringify({
                currentPassword,
                newPassword
            })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erreur lors du changement de mot de passe');
        }
        
        showSuccess('✅ Mot de passe modifié avec succès !');
        
        // Réinitialiser le formulaire
        document.getElementById('currentPassword').value = '';
        document.getElementById('newPassword').value = '';
        document.getElementById('confirmPassword').value = '';
    } catch (error) {
        console.error('Erreur:', error);
        showError(error.message);
    }
}

function validatePassword(password) {
    const minLength = 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecialChar = /[!@#$%^&*]/.test(password);
    
    return password.length >= minLength && 
           hasUpperCase && 
           hasLowerCase && 
           hasNumbers && 
           hasSpecialChar;
}

async function downloadUserData() {
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/export-data`, {
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de l\'export');
        }
        
        const blob = await response.blob();
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `mes-donnees-jo2024-${Date.now()}.json`;
        document.body.appendChild(a);
        a.click();
        window.URL.revokeObjectURL(url);
        document.body.removeChild(a);
        
        showSuccess('Vos données ont été téléchargées');
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de télécharger vos données');
    }
}

function confirmDeleteAccount() {
    const confirmed = confirm(
        '⚠️ ATTENTION ⚠️\n\n' +
        'Êtes-vous sûr de vouloir supprimer votre compte ?\n\n' +
        'Cette action est IRRÉVERSIBLE et entraînera :\n' +
        '• La suppression de toutes vos données personnelles\n' +
        '• La perte de tous vos billets\n' +
        '• L\'annulation de toutes vos commandes'
    );
    
    if (confirmed) {
        const finalConfirm = prompt('Tapez "SUPPRIMER" en majuscules pour confirmer :');
        if (finalConfirm === 'SUPPRIMER') {
            deleteAccount();
        }
    }
}

async function deleteAccount() {
    try {
        const response = await fetch(`${API_BASE_URL}/Compte/delete`, {
            method: 'DELETE',
            credentials: 'include'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de la suppression');
        }
        
        alert('Votre compte a été supprimé. Vous allez être déconnecté.');
        window.location.href = '/';
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de supprimer votre compte');
    }
}

// ===========================
// NAVIGATION
// ===========================

function setupNavigation() {
    const navLinks = document.querySelectorAll('.nav-link');
    
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            e.preventDefault();
            const section = link.dataset.section;
            showSection(section);
            
            // Mettre à jour les liens actifs
            navLinks.forEach(l => l.classList.remove('active'));
            link.classList.add('active');
        });
    });
}

function showSection(sectionName) {
    // Masquer toutes les sections
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });
    
    // Afficher la section demandée
    const section = document.getElementById(sectionName);
    if (section) {
        section.classList.add('active');
    }
}

// ===========================
// UTILITAIRES
// ===========================

function formatDate(dateString) {
    const date = new Date(dateString);
    return date.toLocaleDateString('fr-FR', {
        day: '2-digit',
        month: 'long',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
}

function formatTime(dateString) {
    const date = new Date(dateString);
    return date.toLocaleTimeString('fr-FR', {
        hour: '2-digit',
        minute: '2-digit'
    });
}

function getStatusClass(statut) {
    const statusMap = {
        'Actif': 'status-active',
        'Scanné': 'status-scanned',
        'Payée': 'status-active',
        'Utilisée': 'status-scanned',
        'Annulée': 'status-cancelled'
    };
    return statusMap[statut] || 'status-active';
}

function getStatusText(billet) {
    if (billet.statut === 'Actif') {
        return 'Actif - Prêt à être utilisé';
    } else if (billet.statut === 'Scanné' && billet.dateScan) {
        return `Scanné le ${formatDate(billet.dateScan)}`;
    }
    return escapeHtml(billet.statut);
}

function showSuccess(message) {
    const successDiv = document.getElementById('successMessage');
    if (successDiv) {
        successDiv.textContent = message;
        successDiv.style.display = 'block';
        
        // Auto-masquer après 5 secondes
        setTimeout(() => {
            successDiv.style.display = 'none';
        }, 5000);
    }
}

function showError(message) {
    const errorDiv = document.getElementById('errorMessage');
    if (errorDiv) {
        errorDiv.textContent = '❌ ' + message;
        errorDiv.style.display = 'block';
        
        // Auto-masquer après 5 secondes
        setTimeout(() => {
            errorDiv.style.display = 'none';
        }, 5000);
    }
}

// Protection XSS : échapper les caractères HTML
function escapeHtml(text) {
    if (!text) return '';
    const div = document.createElement('div');
    div.textContent = text;
    return div.innerHTML;
}