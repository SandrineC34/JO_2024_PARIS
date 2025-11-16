// compte.js - Version complète avec authentification JWT

const API_BASE_URL = 'http://localhost:5000/api';

let currentUser = null;
let userTickets = [];
let userOrders = [];
let newsletterPreferences = null;

// ===========================
// INITIALISATION
// ===========================

document.addEventListener('DOMContentLoaded', async () => {
    console.log('📱 Initialisation de la page Mon Compte');
    
    await checkAuthentication();
    await loadUserData();
    
    setupNavigation();
    showSection('tickets');
});

// ===========================
// AUTHENTIFICATION JWT
// ===========================

async function checkAuthentication() {
    try {
        const token = localStorage.getItem('jo_token');
        
        if (!token) {
            console.log('❌ Pas de token trouvé, redirection vers connexion');
            window.location.href = '/connexion.html';
            return;
        }

        // 👉 Appel API avec le token JWT dans le header
        const response = await fetch(`${API_BASE_URL}/Auth/current`, {
            method: 'GET',
            headers: {
                'Authorization': `Bearer ${token}`,
                'Content-Type': 'application/json'
            }
        });
        
        if (!response.ok) {
            console.log('❌ Token invalide ou expiré');
            localStorage.removeItem('jo_token');
            localStorage.removeItem('jo_current_user');
            window.location.href = '/connexion.html';
            return;
        }
        
        currentUser = await response.json();
        updateWelcomeMessage();
        console.log('✅ Utilisateur authentifié:', currentUser);
    } catch (error) {
        console.error('Erreur d\'authentification:', error);
        localStorage.removeItem('jo_token');
        localStorage.removeItem('jo_current_user');
        showError('Erreur de connexion. Veuillez vous reconnecter.');
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
// FONCTION FETCH AUTHENTIFIÉE
// ===========================

async function authenticatedFetch(url, options = {}) {
    const token = localStorage.getItem('jo_token');
    
    if (!token) {
        throw new Error('Non authentifié');
    }

    const authOptions = {
        ...options,
        headers: {
            ...options.headers,
            'Authorization': `Bearer ${token}`,
            'Content-Type': 'application/json'
        }
    };

    const response = await fetch(url, authOptions);
    
    if (response.status === 401) {
        console.log('❌ Session expirée');
        localStorage.removeItem('jo_token');
        localStorage.removeItem('jo_current_user');
        window.location.href = '/connexion.html';
        throw new Error('Session expirée');
    }

    return response;
}

// ===========================
// CHARGEMENT DES DONNÉES
// ===========================

async function loadUserData() {
    try {
        await Promise.all([
            loadTickets(),
            loadOrders(),
            loadUserProfile(),
            loadNewsletterPreferences()
        ]);
    } catch (error) {
        console.error('Erreur de chargement des données:', error);
        showError('Impossible de charger vos données');
    }
}

// ===========================
// GESTION DES BILLETS
// ===========================

async function loadTickets() {
    const loader = document.getElementById('ticketsLoader');
    const container = document.getElementById('ticketsContainer');
    
    try {
        if (loader) loader.style.display = 'block';
        
        const response = await authenticatedFetch(`${API_BASE_URL}/Billets`);
        
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
        if (loader) loader.style.display = 'none';
    }
}

function displayTickets() {
    const container = document.getElementById('ticketsContainer');
    
    if (!container) return;
    
    if (userTickets.length === 0) {
        container.innerHTML = `
            <div style="text-align: center; padding: 40px; color: #666;">
                <p style="font-size: 1.2em;">🔭 Vous n'avez pas encore de billets</p>
                <p style="margin-top: 10px;">Découvrez nos offres et réservez vos places pour les JO 2024 !</p>
                <a href="/offres.html" class="btn btn-primary" style="margin-top: 20px; display: inline-block;">
                    🎫 Découvrir les offres
                </a>
            </div>
        `;
        return;
    }
    
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

function toggleTicketDetails(card) {
    const details = card.querySelector('.ticket-details');
    const isOpen = details.style.display === 'block';
    
    document.querySelectorAll('.ticket-details').forEach(d => {
        d.style.display = 'none';
    });
    
    if (!isOpen) {
        details.style.display = 'block';
    }
}

async function viewQRCode(event, billetId) {
    event.stopPropagation();
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Billets/${billetId}`);
        
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
    
    modal.addEventListener('click', (e) => {
        if (e.target === modal) {
            modal.remove();
        }
    });
}

async function downloadPDF(event, billetId) {
    event.stopPropagation();
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Billets/${billetId}/download`, {
            method: 'POST'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors du téléchargement');
        }
        
        const result = await response.json();
        showSuccess('PDF généré avec succès ! Téléchargement en cours...');
        
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
        const response = await authenticatedFetch(`${API_BASE_URL}/Billets/${billetId}/email`, {
            method: 'POST'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de l\'envoi');
        }
        
        const result = await response.json();
        showSuccess(result.message || 'Email envoyé avec succès !');
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible d\'envoyer l\'email');
    }
}

// ===========================
// GESTION DES COMMANDES
// ===========================

async function loadOrders() {
    const loader = document.getElementById('ordersLoader');
    const tbody = document.getElementById('ordersContainer');
    
    try {
        if (loader) loader.style.display = 'block';
        
        const response = await authenticatedFetch(`${API_BASE_URL}/Commandes`);
        
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
        if (loader) loader.style.display = 'none';
    }
}

function displayOrders() {
    const tbody = document.getElementById('ordersContainer');
    
    if (!tbody) return;
    
    if (userOrders.length === 0) {
        tbody.innerHTML = `
            <tr>
                <td colspan="5" style="text-align: center; padding: 40px; color: #666;">
                    🔭 Aucune commande pour le moment<br>
                    <small style="margin-top: 10px; display: block;">
                        Vos commandes apparaîtront ici après votre premier achat
                    </small>
                </td>
            </tr>
        `;
        return;
    }
    
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

// ===========================
// GESTION DU PROFIL
// ===========================

async function loadUserProfile() {
    const loader = document.getElementById('settingsLoader');
    const form = document.getElementById('profileForm');
    
    try {
        if (loader) loader.style.display = 'block';
        
        const response = await authenticatedFetch(`${API_BASE_URL}/Compte/profile`);
        
        if (!response.ok) {
            throw new Error('Erreur lors du chargement du profil');
        }
        
        const profile = await response.json();
        fillProfileForm(profile);
        
        if (form) form.style.display = 'block';
    } catch (error) {
        console.error('Erreur:', error);
        if (currentUser) {
            fillProfileForm({
                prenom: currentUser.prenom,
                nom: currentUser.nom,
                email: currentUser.email || ''
            });
            if (form) form.style.display = 'block';
        }
    } finally {
        if (loader) loader.style.display = 'none';
    }
}

function fillProfileForm(profile) {
    document.getElementById('firstName').value = profile.prenom || '';
    document.getElementById('lastName').value = profile.nom || '';
    document.getElementById('email').value = profile.email || '';
}

async function saveSettings(event) {
    event.preventDefault();
    
    const formData = {
        prenom: document.getElementById('firstName').value,
        nom: document.getElementById('lastName').value,
        email: document.getElementById('email').value
    };
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Compte/profile`, {
            method: 'PUT',
            body: JSON.stringify(formData)
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de la mise à jour');
        }
        
        showSuccess('Informations mises à jour avec succès !');
        
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
    
    if (newPassword !== confirmPassword) {
        showError('Les mots de passe ne correspondent pas');
        return;
    }
    
    if (!validatePassword(newPassword)) {
        showError('Le mot de passe ne respecte pas les critères de sécurité');
        return;
    }
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Auth/change-password`, {
            method: 'POST',
            body: JSON.stringify({
                currentPassword,
                newPassword
            })
        });
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erreur lors du changement de mot de passe');
        }
        
        showSuccess('Mot de passe modifié avec succès !');
        
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

// ===========================
// NEWSLETTER - GESTION COMPLÈTE
// ===========================

async function loadNewsletterPreferences() {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Newsletter/preferences`);
        
        if (!response.ok) {
            newsletterPreferences = {
                estAbonne: false,
                categories: {
                    sports: false,
                    evenements: false,
                    billets: false
                },
                sports: []
            };
        } else {
            newsletterPreferences = await response.json();
        }
        
        displayNewsletterForm();
    } catch (error) {
        console.error('Erreur chargement newsletter:', error);
        newsletterPreferences = {
            estAbonne: false,
            categories: { sports: false, evenements: false, billets: false },
            sports: []
        };
        displayNewsletterForm();
    }
}

function displayNewsletterForm() {
    const newsletterAbonne = document.getElementById('newsletterAbonne');
    const categoriesSection = document.getElementById('categoriesSection');
    
    if (!newsletterAbonne) return;
    
    newsletterAbonne.checked = newsletterPreferences?.estAbonne || false;
    
    if (categoriesSection) {
        categoriesSection.innerHTML = `
            <h4 style="margin-bottom: 15px; color: #333;">Catégories d'intérêt :</h4>
            <div style="margin-bottom: 15px;">
                <label class="checkbox-label" style="display: block; margin: 10px 0; background: #fff; padding: 10px; border-radius: 6px; border: 1px solid #ddd;">
                    <input type="checkbox" id="category_sport" name="category_sport" 
                        ${newsletterPreferences?.categories?.sports ? 'checked' : ''}>
                    <span style="font-weight: 500;">🏅 Sports et compétitions</span>
                </label>
                <div id="sportsSubcategories" style="display: ${newsletterPreferences?.categories?.sports ? 'block' : 'none'}; margin-left: 30px; padding: 15px; background: #f0fff4; border-left: 3px solid #28a745; border-radius: 6px;">
                    <p style="font-size: 14px; color: #666; margin-bottom: 10px; font-weight: 600;">
                        Sélectionnez vos sports favoris :
                    </p>
                    <label style="display: block; margin: 8px 0; cursor: pointer;">
                        <input type="checkbox" id="sport_natation" name="sport_natation"
                            ${hasSport('natation') ? 'checked' : ''}>
                        🏊 Natation
                    </label>
                    <label style="display: block; margin: 8px 0; cursor: pointer;">
                        <input type="checkbox" id="sport_athletisme" name="sport_athletisme"
                            ${hasSport('athletisme') ? 'checked' : ''}>
                        🏃 Athlétisme
                    </label>
                    <label style="display: block; margin: 8px 0; cursor: pointer;">
                        <input type="checkbox" id="sport_basketball" name="sport_basketball"
                            ${hasSport('basketball') ? 'checked' : ''}>
                        🏀 Basketball
                    </label>
                    <label style="display: block; margin: 8px 0; cursor: pointer;">
                        <input type="checkbox" id="sport_surf" name="sport_surf"
                            ${hasSport('surf') ? 'checked' : ''}>
                        🏄 Surf
                    </label>
                    <label style="display: block; margin: 8px 0; cursor: pointer;">
                        <input type="checkbox" id="sport_gymnastique" name="sport_gymnastique"
                            ${hasSport('gymnastique') ? 'checked' : ''}>
                        🤸 Gymnastique
                    </label>
                </div>
            </div>
            <label class="checkbox-label" style="display: block; margin: 10px 0; background: #fff; padding: 10px; border-radius: 6px; border: 1px solid #ddd;">
                <input type="checkbox" id="category_evenements" name="category_evenements"
                    ${newsletterPreferences?.categories?.evenements ? 'checked' : ''}>
                <span style="font-weight: 500;">🎉 Événements et actualités</span>
            </label>
            <label class="checkbox-label" style="display: block; margin: 10px 0; background: #fff; padding: 10px; border-radius: 6px; border: 1px solid #ddd;">
                <input type="checkbox" id="category_billets" name="category_billets"
                    ${newsletterPreferences?.categories?.billets ? 'checked' : ''}>
                <span style="font-weight: 500;">🎫 Offres spéciales billets</span>
            </label>
        `;
        
        updateCategoriesVisibility();
        setupSportsToggle();
    }
    
    newsletterAbonne.addEventListener('change', updateCategoriesVisibility);
}

function hasSport(sportId) {
    if (!newsletterPreferences?.sports || !Array.isArray(newsletterPreferences.sports)) {
        return false;
    }
    return newsletterPreferences.sports.some(sport => sport.id === sportId);
}

function setupSportsToggle() {
    const categorySport = document.getElementById('category_sport');
    const sportsSubcategories = document.getElementById('sportsSubcategories');
    
    if (!categorySport || !sportsSubcategories) return;
    
    categorySport.addEventListener('change', function() {
        const isChecked = this.checked;
        sportsSubcategories.style.display = isChecked ? 'block' : 'none';
        
        document.querySelectorAll('[name^="sport_"]').forEach(cb => {
            cb.disabled = !isChecked;
            if (!isChecked) cb.checked = false;
        });
    });
    
    const categoryIsChecked = categorySport.checked;
    document.querySelectorAll('[name^="sport_"]').forEach(cb => {
        cb.disabled = !categoryIsChecked;
    });
}

function updateCategoriesVisibility() {
    const newsletterAbonne = document.getElementById('newsletterAbonne');
    const categoriesSection = document.getElementById('categoriesSection');
    const categoryInputs = categoriesSection?.querySelectorAll('input[type="checkbox"]:not([name^="sport_"])');
    const sportInputs = document.querySelectorAll('[name^="sport_"]');
    const categorySport = document.getElementById('category_sport');
    
    if (!newsletterAbonne || !categoriesSection) return;
    
    const isSubscribed = newsletterAbonne.checked;
    
    if (categoryInputs) {
        categoryInputs.forEach(input => {
            input.disabled = !isSubscribed;
            input.closest('label').style.opacity = isSubscribed ? '1' : '0.5';
        });
    }
    
    if (sportInputs && categorySport) {
        const sportCategoryChecked = isSubscribed && categorySport.checked;
        sportInputs.forEach(input => {
            input.disabled = !sportCategoryChecked;
        });
    }
    
    if (!isSubscribed) {
        if (categoryInputs) {
            categoryInputs.forEach(input => input.checked = false);
        }
        if (sportInputs) {
            sportInputs.forEach(input => input.checked = false);
        }
        const sportsSubcategories = document.getElementById('sportsSubcategories');
        if (sportsSubcategories) {
            sportsSubcategories.style.display = 'none';
        }
    }
}

async function saveNewsletterPreferences(event) {
    event.preventDefault();
    
    const newsletterAbonne = document.getElementById('newsletterAbonne');
    const estAbonne = newsletterAbonne?.checked || false;
    const categorySport = document.getElementById('category_sport')?.checked || false;
    const categoryEvenements = document.getElementById('category_evenements')?.checked || false;
    const categoryBillets = document.getElementById('category_billets')?.checked || false;
    
    let selectedSports = [];
    if (estAbonne && categorySport) {
        const sportsMap = {
            'sport_natation': 'Natation',
            'sport_athletisme': 'Athlétisme',
            'sport_basketball': 'Basketball',
            'sport_surf': 'Surf',
            'sport_gymnastique': 'Gymnastique'
        };
        
        Object.keys(sportsMap).forEach(sportId => {
            const checkbox = document.getElementById(sportId);
            if (checkbox && checkbox.checked) {
                selectedSports.push({
                    id: sportId.replace('sport_', ''),
                    name: sportsMap[sportId]
                });
            }
        });
    }
    
    const updateDto = {
        estAbonne: estAbonne,
        categories: {
            sports: estAbonne && categorySport,
            evenements: estAbonne && categoryEvenements,
            billets: estAbonne && categoryBillets
        },
        sports: selectedSports
    };
    
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Newsletter/preferences`, {
            method: 'PUT',
            body: JSON.stringify(updateDto)
        });
        
        if (!response.ok) throw new Error('Erreur lors de la mise à jour');
        
        const result = await response.json();
        
        if (estAbonne) {
            let msg = 'Préférences mises à jour ! ';
            if (selectedSports.length > 0) {
                const sportsNames = selectedSports.map(s => s.name).join(', ');
                msg += `\nSports suivis : ${sportsNames}`;
            }
            showSuccess(msg);
        } else {
            showSuccess('Vous avez été désinscrit de la newsletter.');
        }
        
        await loadNewsletterPreferences();
    } catch (error) {
        console.error('Erreur:', error);
        showError('Impossible de mettre à jour vos préférences newsletter');
    }
}

// ===========================
// RGPD
// ===========================

async function downloadUserData() {
    try {
        const response = await authenticatedFetch(`${API_BASE_URL}/Compte/export-data`);
        
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
        '• L\'annulation de toutes vos commandes\n' +
        '• La désinscription de la newsletter'
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
        const response = await authenticatedFetch(`${API_BASE_URL}/Compte/delete`, {
            method: 'DELETE'
        });
        
        if (!response.ok) {
            throw new Error('Erreur lors de la suppression');