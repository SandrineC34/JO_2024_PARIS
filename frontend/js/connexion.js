// Variables globales
let users = JSON.parse(localStorage.getItem('jo_users') || '[]');
let currentUser = JSON.parse(localStorage.getItem('jo_current_user') || 'null');

// Configuration pour simulation backend (à remplacer par vraies URLs d'API plus tard)
const CONFIG = {
    useLocalStorage: true, // true pour localStorage, false pour API
    apiBaseUrl: 'https://votre-api.com/api'
};

// Initialisation au chargement de la page
document.addEventListener('DOMContentLoaded', () => {
    // Initialisation de l'authentification
    initializeAuth();
    
    // Gestion de la newsletter
    initializeNewsletterToggle();
    
    // Gestion des événements de saisie
    initializeInputValidation();
});

// Initialisation de l'authentification
function initializeAuth() {
    // Masquer le loading au démarrage
    hideLoading();
    
    // NE PAS rediriger automatiquement - laisser l'utilisateur sur la page
    // L'utilisateur peut vouloir créer un autre compte ou se déconnecter
    
    // Animation d'entrée
    setTimeout(addEntryAnimation, 100);
}

// Initialisation du toggle newsletter
function initializeNewsletterToggle() {
    const newsletterConsent = document.getElementById('newsletterConsent');
    const newsletterCategories = document.getElementById('newsletterCategories');

    if (newsletterConsent && newsletterCategories) {
        // Afficher les catégories si la checkbox est cochée
        newsletterCategories.style.display = newsletterConsent.checked ? 'block' : 'none';

        // Gérer le changement d'état
        newsletterConsent.addEventListener('change', () => {
            newsletterCategories.style.display = newsletterConsent.checked ? 'block' : 'none';
        });
    }
}

// Affichage des formulaires
function showLoginForm() {
    document.getElementById('loginForm').style.display = 'block';
    document.getElementById('registerForm').style.display = 'none';
    document.getElementById('forgotPasswordForm').style.display = 'none';
    clearMessages();
    resetForms();
}

function showRegisterForm() {
    document.getElementById('loginForm').style.display = 'none';
    document.getElementById('registerForm').style.display = 'block';
    document.getElementById('forgotPasswordForm').style.display = 'none';
    clearMessages();
    resetForms();
}

function showForgotPassword() {
    document.getElementById('loginForm').style.display = 'none';
    document.getElementById('registerForm').style.display = 'none';
    document.getElementById('forgotPasswordForm').style.display = 'block';
    clearMessages();
    resetForms();
}

// Réinitialiser les formulaires
function resetForms() {
    // Réinitialiser tous les formulaires
    document.querySelectorAll('form').forEach(form => {
        form.reset();
    });
    
    // Réactiver le bouton d'inscription
    const registerBtn = document.getElementById('registerBtn');
    if (registerBtn) {
        registerBtn.disabled = false;
    }
    
    // Réinitialiser l'affichage des catégories newsletter
    initializeNewsletterToggle();
}

// Gestion des messages
function showMessage(type, message) {
    const messageEl = document.getElementById(type + 'Message');
    const otherType = type === 'success' ? 'error' : 'success';
    const otherMessageEl = document.getElementById(otherType + 'Message');
    
    // Masquer l'autre message
    otherMessageEl.style.display = 'none';
    
    // Afficher le message
    messageEl.textContent = message;
    messageEl.style.display = 'block';
    
    // Faire défiler vers le message
    messageEl.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    
    // Auto-hide après 8 secondes (sauf pour les succès de connexion/inscription)
    if (!message.includes('Bienvenue') && !message.includes('créé avec succès')) {
        setTimeout(() => {
            messageEl.style.display = 'none';
        }, 8000);
    }
}

function clearMessages() {
    document.getElementById('successMessage').style.display = 'none';
    document.getElementById('errorMessage').style.display = 'none';
    clearFieldErrors();
}

function clearFieldErrors() {
    document.querySelectorAll('.error-message').forEach(error => {
        error.style.display = 'none';
        error.textContent = '';
    });
    document.querySelectorAll('input.error').forEach(input => {
        input.classList.remove('error');
    });
}

function showFieldError(fieldName, message) {
    const field = document.getElementById(fieldName);
    const errorEl = document.getElementById(fieldName + 'Error');
    
    if (field) {
        field.classList.add('error');
        field.focus();
    }
    
    if (errorEl) {
        errorEl.textContent = message;
        errorEl.style.display = 'block';
    }
}

// Validation améliorée du mot de passe
function validatePassword(password) {
    const minLength = password.length >= 8;
    const hasUpperCase = /[A-Z]/.test(password);
    const hasLowerCase = /[a-z]/.test(password);
    const hasNumbers = /\d/.test(password);
    const hasSpecialChar = /[!@#$%^&*]/.test(password);

    return {
        isValid: minLength && hasUpperCase && hasLowerCase && hasNumbers && hasSpecialChar,
        errors: {
            length: !minLength,
            uppercase: !hasUpperCase,
            lowercase: !hasLowerCase,
            numbers: !hasNumbers,
            special: !hasSpecialChar
        }
    };
}

function checkPasswordStrength() {
    const password = document.getElementById('registerPassword').value;
    const registerBtn = document.getElementById('registerBtn');
    const requirements = document.querySelector('.password-requirements');
    
    if (password.length === 0) {
        registerBtn.disabled = false;
        if (requirements) {
            requirements.style.color = '#666';
        }
        return;
    }

    const validation = validatePassword(password);
    registerBtn.disabled = !validation.isValid;
    
    // Mise à jour visuelle des critères
    if (requirements) {
        const criteria = [
            { test: !validation.errors.length, text: '• Minimum 8 caractères' },
            { test: !validation.errors.uppercase, text: '• Au moins une majuscule' },
            { test: !validation.errors.lowercase, text: '• Au moins une minuscule' },
            { test: !validation.errors.numbers, text: '• Au moins un chiffre' },
            { test: !validation.errors.special, text: '• Au moins un caractère spécial (!@#$%^&*)' }
        ];
        
        let html = 'Le mot de passe doit contenir :<br>';
        criteria.forEach(criterion => {
            const color = criterion.test ? '#28a745' : '#dc3545';
            html += `<span style="color: ${color}">${criterion.text}</span><br>`;
        });
        
        requirements.innerHTML = html;
        requirements.style.color = validation.isValid ? '#28a745' : '#dc3545';
    }
    
    // Effacer les erreurs si le mot de passe devient valide
    if (validation.isValid) {
        const errorEl = document.getElementById('registerPasswordError');
        if (errorEl) {
            errorEl.style.display = 'none';
        }
        document.getElementById('registerPassword').classList.remove('error');
    }
}

// Validation de l'email améliorée
function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Gestion de la connexion améliorée
async function handleLogin(event) {
    event.preventDefault();
    clearMessages();

    const email = document.getElementById('loginEmail').value.trim();
    const password = document.getElementById('loginPassword').value;

    // Validation côté client améliorée
    let hasErrors = false;

    if (!email) {
        showFieldError('loginEmail', 'L\'email est obligatoire');
        hasErrors = true;
    } else if (!validateEmail(email)) {
        showFieldError('loginEmail', 'Format d\'email invalide');
        hasErrors = true;
    }

    if (!password) {
        showFieldError('loginPassword', 'Le mot de passe est obligatoire');
        hasErrors = true;
    }

    if (hasErrors) return;

    showLoading();

    try {
        if (CONFIG.useLocalStorage) {
            // Simulation avec localStorage
            await simulateLogin(email, password);
        } else {
            // Vraie API (à implémenter plus tard)
            await apiLogin(email, password);
        }
    } catch (error) {
        console.error('Erreur lors de la connexion:', error);
        if (!error.message.includes('User not found') && !error.message.includes('Wrong password')) {
            showMessage('error', 'Une erreur technique est survenue. Veuillez réessayer.');
        }
    } finally {
        hideLoading();
    }
}

// Simulation de connexion avec localStorage
async function simulateLogin(email, password) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            const user = users.find(u => u.email === email);
            
            if (!user) {
                showFieldError('loginEmail', 'Aucun compte trouvé avec cet email');
                reject(new Error('User not found'));
                return;
            }

            if (user.password !== password) {
                showFieldError('loginPassword', 'Mot de passe incorrect');
                reject(new Error('Wrong password'));
                return;
            }

            // Mise à jour de la dernière connexion
            user.lastLogin = new Date().toISOString();
            updateUserInStorage(user);

            // Connexion réussie
            currentUser = user;
            localStorage.setItem('jo_current_user', JSON.stringify(currentUser));
            
            showMessage('success', `Bienvenue ${user.firstName} ! Connexion réussie.`);
            
            // Redirection après 2 secondes
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 2000);

            resolve(user);
        }, 1500);
    });
}

// Gestion de l'inscription améliorée
async function handleRegister(event) {
    event.preventDefault();
    clearMessages();

    const firstName = document.getElementById('firstName').value.trim();
    const lastName = document.getElementById('lastName').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

    // Validation complète
    if (!validateRegisterForm(firstName, lastName, email, password, confirmPassword)) {
        return;
    }

    showLoading();

    try {
        if (CONFIG.useLocalStorage) {
            await simulateRegister(firstName, lastName, email, password);
        } else {
            await apiRegister(firstName, lastName, email, password);
        }
    } catch (error) {
        console.error('Erreur lors de l\'inscription:', error);
        if (!error.message.includes('déjà utilisé') && !error.message.includes('RGPD')) {
            showMessage('error', 'Une erreur technique est survenue. Veuillez réessayer.');
        }
    } finally {
        hideLoading();
    }
}

// Validation complète du formulaire d'inscription
function validateRegisterForm(firstName, lastName, email, password, confirmPassword) {
    let hasErrors = false;

    // Validation prénom
    if (!firstName) {
        showFieldError('firstName', 'Le prénom est obligatoire');
        hasErrors = true;
    } else if (firstName.length < 2) {
        showFieldError('firstName', 'Le prénom doit contenir au moins 2 caractères');
        hasErrors = true;
    } else if (!/^[a-zA-ZÀ-ÿ\s-']+$/.test(firstName)) {
        showFieldError('firstName', 'Le prénom ne peut contenir que des lettres, espaces, tirets et apostrophes');
        hasErrors = true;
    }

    // Validation nom
    if (!lastName) {
        showFieldError('lastName', 'Le nom est obligatoire');
        hasErrors = true;
    } else if (lastName.length < 2) {
        showFieldError('lastName', 'Le nom doit contenir au moins 2 caractères');
        hasErrors = true;
    } else if (!/^[a-zA-ZÀ-ÿ\s-']+$/.test(lastName)) {
        showFieldError('lastName', 'Le nom ne peut contenir que des lettres, espaces, tirets et apostrophes');
        hasErrors = true;
    }

    // Validation email
    if (!email) {
        showFieldError('registerEmail', 'L\'email est obligatoire');
        hasErrors = true;
    } else if (!validateEmail(email)) {
        showFieldError('registerEmail', 'Format d\'email invalide');
        hasErrors = true;
    } else if (users.some(u => u.email.toLowerCase() === email.toLowerCase())) {
        showFieldError('registerEmail', 'Cette adresse email est déjà utilisée');
        hasErrors = true;
    }

    // Validation mot de passe
    const passwordValidation = validatePassword(password);
    if (!password) {
        showFieldError('registerPassword', 'Le mot de passe est obligatoire');
        hasErrors = true;
    } else if (!passwordValidation.isValid) {
        showFieldError('registerPassword', 'Le mot de passe ne respecte pas les critères requis');
        hasErrors = true;
    }

    // Confirmation du mot de passe
    if (!confirmPassword) {
        showFieldError('confirmPassword', 'La confirmation du mot de passe est obligatoire');
        hasErrors = true;
    } else if (password !== confirmPassword) {
        showFieldError('confirmPassword', 'Les mots de passe ne correspondent pas');
        hasErrors = true;
    }

    // Validation RGPD (obligatoire)
    const rgpdConsent = document.getElementById('rgpdConsent');
    if (rgpdConsent && !rgpdConsent.checked) {
        showMessage('error', 'Vous devez accepter la politique de confidentialité pour créer un compte.');
        hasErrors = true;
    }

    return !hasErrors;
}

// Simulation d'inscription avec localStorage
async function simulateRegister(firstName, lastName, email, password) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            // Vérification finale de l'email (case insensitive)
            if (users.some(u => u.email.toLowerCase() === email.toLowerCase())) {
                showFieldError('registerEmail', 'Cette adresse email est déjà utilisée');
                reject(new Error('Email déjà utilisé'));
                return;
            }

            // Vérification RGPD
            const rgpdConsent = document.getElementById('rgpdConsent');
            if (!rgpdConsent || !rgpdConsent.checked) {
                showMessage('error', 'Vous devez accepter la politique de confidentialité.');
                reject(new Error('RGPD non accepté'));
                return;
            }

            // Récupération des préférences newsletter
            const newsletterConsent = document.getElementById('newsletterConsent')?.checked || false;
            const newsletterPreferences = {
                subscribed: newsletterConsent,
                categories: {
                    sport: newsletterConsent && document.querySelector('[name="category_sport"]')?.checked || false,
                    evenements: newsletterConsent && document.querySelector('[name="category_evenements"]')?.checked || false,
                    billets: newsletterConsent && document.querySelector('[name="category_billets"]')?.checked || false
                },
                subscribedAt: newsletterConsent ? new Date().toISOString() : null
            };

            // Création de l'utilisateur
            const newUser = {
                id: generateUserId(),
                firstName: capitalizeFirstLetter(firstName),
                lastName: capitalizeFirstLetter(lastName),
                email: email.toLowerCase(),
                password,
                securityKey: generateSecurityKey(),
                newsletter: newsletterPreferences,
                rgpdAcceptedAt: new Date().toISOString(),
                createdAt: new Date().toISOString(),
                lastLogin: null,
                isActive: true
            };

            users.push(newUser);
            localStorage.setItem('jo_users', JSON.stringify(users));
            
            // Connexion automatique
            currentUser = newUser;
            localStorage.setItem('jo_current_user', JSON.stringify(currentUser));

            showMessage('success', 
                `Félicitations ${newUser.firstName} ! Votre compte a été créé avec succès. ` +
                `Vous êtes maintenant connecté(e).`
            );

            // Redirection après 3 secondes
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 3000);

            resolve(newUser);
        }, 2000);
    });
}

// Gestion du mot de passe oublié améliorée
async function handleForgotPassword(event) {
    event.preventDefault();
    clearMessages();

    const email = document.getElementById('forgotEmail').value.trim();

    if (!email) {
        showFieldError('forgotEmail', 'L\'email est obligatoire');
        return;
    }

    if (!validateEmail(email)) {
        showFieldError('forgotEmail', 'Format d\'email invalide');
        return;
    }

    showLoading();

    try {
        const user = users.find(u => u.email.toLowerCase() === email.toLowerCase());
        
        setTimeout(() => {
            hideLoading();
            
            if (!user) {
                showFieldError('forgotEmail', 'Aucun compte trouvé avec cette adresse email');
                return;
            }

            // Générer un token de réinitialisation (simulation)
            const resetToken = generateResetToken();
            user.resetToken = resetToken;
            user.resetTokenExpiry = new Date(Date.now() + 3600000).toISOString(); // 1 heure
            updateUserInStorage(user);

            showMessage('success', 
                `Un lien de réinitialisation a été envoyé à ${email}. ` +
                `Veuillez vérifier votre boîte de réception et vos spams. ` +
                `Le lien expire dans 1 heure.`
            );

            // Retour au formulaire de connexion après 5 secondes
            setTimeout(() => {
                showLoginForm();
            }, 5000);
        }, 1500);
    } catch (error) {
        hideLoading();
        console.error('Erreur mot de passe oublié:', error);
        showMessage('error', 'Une erreur technique est survenue. Veuillez réessayer.');
    }
}

// Fonctions utilitaires améliorées
function generateUserId() {
    return 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}

function generateSecurityKey() {
    return 'sk_' + Math.random().toString(36).substr(2, 20) + Date.now().toString(36);
}

function generateResetToken() {
    return 'rst_' + Math.random().toString(36).substr(2, 32);
}

function capitalizeFirstLetter(string) {
    return string.charAt(0).toUpperCase() + string.slice(1).toLowerCase();
}

function updateUserInStorage(user) {
    const userIndex = users.findIndex(u => u.id === user.id);
    if (userIndex !== -1) {
        users[userIndex] = user;
        localStorage.setItem('jo_users', JSON.stringify(users));
    }
}

// Fonctions de chargement
function showLoading() {
    const loadingSection = document.getElementById('loadingSection');
    if (loadingSection) {
        loadingSection.style.display = 'block';
    }
    
    // Désactiver tous les boutons
    document.querySelectorAll('button[type="submit"]').forEach(btn => {
        btn.disabled = true;
    });
}

function hideLoading() {
    const loadingSection = document.getElementById('loadingSection');
    if (loadingSection) {
        loadingSection.style.display = 'none';
    }
    
    // Réactiver tous les boutons
    document.querySelectorAll('button[type="submit"]').forEach(btn => {
        btn.disabled = false;
    });
}

// Animation d'entrée améliorée
function addEntryAnimation() {
    const authContainer = document.querySelector('.auth-container');
    if (authContainer) {
        authContainer.style.opacity = '0';
        authContainer.style.transform = 'translateY(30px)';
        authContainer.style.transition = 'opacity 0.6s ease, transform 0.6s ease';
        
        setTimeout(() => {
            authContainer.style.opacity = '1';
            authContainer.style.transform = 'translateY(0)';
        }, 100);
    }
}

// Gestion des événements de saisie pour améliorer l'UX
function initializeInputValidation() {
    // Effacer les erreurs lors de la saisie
    document.querySelectorAll('input').forEach(input => {
        input.addEventListener('input', () => {
            if (input.classList.contains('error')) {
                input.classList.remove('error');
                const errorEl = document.getElementById(input.id + 'Error');
                if (errorEl) {
                    errorEl.style.display = 'none';
                }
            }
        });
        
        // Validation en temps réel pour l'email
        if (input.type === 'email') {
            input.addEventListener('blur', () => {
                const email = input.value.trim();
                if (email && !validateEmail(email)) {
                    showFieldError(input.id, 'Format d\'email invalide');
                }
            });
        }
    });
    
    // Gestion spéciale pour la confirmation de mot de passe
    const confirmPasswordField = document.getElementById('confirmPassword');
    if (confirmPasswordField) {
        confirmPasswordField.addEventListener('input', () => {
            const password = document.getElementById('registerPassword').value;
            const confirmPassword = confirmPasswordField.value;
            
            if (confirmPassword && password !== confirmPassword) {
                showFieldError('confirmPassword', 'Les mots de passe ne correspondent pas');
            }
        });
    }
}

// Fonctions pour futures intégrations API
async function apiLogin(email, password) {
    const response = await fetch(`${CONFIG.apiBaseUrl}/auth/login`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    
    if (!response.ok) {
        throw new Error(data.message || 'Erreur de connexion');
    }
    
    return data;
}

async function apiRegister(firstName, lastName, email, password) {
    const response = await fetch(`${CONFIG.apiBaseUrl}/auth/register`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ firstName, lastName, email, password })
    });
    
    const data = await response.json();
    
    if (!response.ok) {
        throw new Error(data.message || 'Erreur lors de l\'inscription');
    }
    
    return data;
}

// Fonction de déconnexion (utile pour les autres pages)
function logout() {
    currentUser = null;
    localStorage.removeItem('jo_current_user');
    window.location.href = 'connexion.html';
}

// Export des fonctions utiles (si besoin pour d'autres fichiers)
window.JO_Auth = {
    getCurrentUser: () => currentUser,
    logout,
    isLoggedIn: () => currentUser !== null
};