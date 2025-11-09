// connexion.js - Version mise à jour avec gestion newsletter

// Variables globales
let users = JSON.parse(localStorage.getItem('jo_users') || '[]');
let currentUser = JSON.parse(localStorage.getItem('jo_current_user') || 'null');

// Configuration pour simulation backend
const CONFIG = {
    useLocalStorage: true,
    apiBaseUrl: '/api'
};

// Initialisation au chargement de la page
document.addEventListener('DOMContentLoaded', () => {
    initializeAuth();
    initializeNewsletterToggle();
    initializeInputValidation();
});

// Initialisation de l'authentification
function initializeAuth() {
    hideLoading();
    setTimeout(addEntryAnimation, 100);
}

// ⭐ MODIFICATION: Initialisation du toggle newsletter (NON coché par défaut)
function initializeNewsletterToggle() {
    const newsletterConsent = document.getElementById('newsletterConsent');
    const newsletterCategories = document.getElementById('newsletterCategories');

    if (newsletterConsent && newsletterCategories) {
        // ✅ Par défaut: NON coché (conforme aux user stories)
        newsletterConsent.checked = false;
        newsletterCategories.style.display = 'none';

        // Gérer le changement d'état
        newsletterConsent.addEventListener('change', () => {
            newsletterCategories.style.display = newsletterConsent.checked ? 'block' : 'none';
            
            // Décocher toutes les catégories si on décoche la newsletter
            if (!newsletterConsent.checked) {
                document.querySelectorAll('[name^="category_"]').forEach(cb => {
                    cb.checked = false;
                });
            }
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
    document.querySelectorAll('form').forEach(form => {
        form.reset();
    });
    
    const registerBtn = document.getElementById('registerBtn');
    if (registerBtn) {
        registerBtn.disabled = false;
    }
    
    initializeNewsletterToggle();
}

// Gestion des messages
function showMessage(type, message) {
    const messageEl = document.getElementById(type + 'Message');
    const otherType = type === 'success' ? 'error' : 'success';
    const otherMessageEl = document.getElementById(otherType + 'Message');
    
    otherMessageEl.style.display = 'none';
    
    messageEl.textContent = message;
    messageEl.style.display = 'block';
    
    messageEl.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
    
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
    
    if (validation.isValid) {
        const errorEl = document.getElementById('registerPasswordError');
        if (errorEl) {
            errorEl.style.display = 'none';
        }
        document.getElementById('registerPassword').classList.remove('error');
    }
}

function validateEmail(email) {
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return emailRegex.test(email);
}

// Gestion de la connexion
async function handleLogin(event) {
    event.preventDefault();
    clearMessages();

    const email = document.getElementById('loginEmail').value.trim();
    const password = document.getElementById('loginPassword').value;

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
            await simulateLogin(email, password);
        } else {
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

            user.lastLogin = new Date().toISOString();
            updateUserInStorage(user);

            currentUser = user;
            localStorage.setItem('jo_current_user', JSON.stringify(currentUser));
            
            showMessage('success', `Bienvenue ${user.firstName} ! Connexion réussie.`);
            
            setTimeout(() => {
                window.location.href = 'index.html';
            }, 2000);

            resolve(user);
        }, 1500);
    });
}

// ⭐ MODIFICATION: Gestion de l'inscription avec newsletter améliorée
async function handleRegister(event) {
    event.preventDefault();
    clearMessages();

    const firstName = document.getElementById('firstName').value.trim();
    const lastName = document.getElementById('lastName').value.trim();
    const email = document.getElementById('registerEmail').value.trim();
    const password = document.getElementById('registerPassword').value;
    const confirmPassword = document.getElementById('confirmPassword').value;

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

function validateRegisterForm(firstName, lastName, email, password, confirmPassword) {
    let hasErrors = false;

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

    const passwordValidation = validatePassword(password);
    if (!password) {
        showFieldError('registerPassword', 'Le mot de passe est obligatoire');
        hasErrors = true;
    } else if (!passwordValidation.isValid) {
        showFieldError('registerPassword', 'Le mot de passe ne respecte pas les critères requis');
        hasErrors = true;
    }

    if (!confirmPassword) {
        showFieldError('confirmPassword', 'La confirmation du mot de passe est obligatoire');
        hasErrors = true;
    } else if (password !== confirmPassword) {
        showFieldError('confirmPassword', 'Les mots de passe ne correspondent pas');
        hasErrors = true;
    }

    // ⭐ Validation RGPD (obligatoire)
    const rgpdConsent = document.getElementById('rgpdConsent');
    if (rgpdConsent && !rgpdConsent.checked) {
        showMessage('error', '⚠️ Vous devez accepter la politique de confidentialité pour créer un compte.');
        hasErrors = true;
    }

    return !hasErrors;
}

// ⭐ MODIFICATION: Simulation d'inscription avec newsletter détaillée
async function simulateRegister(firstName, lastName, email, password) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            if (users.some(u => u.email.toLowerCase() === email.toLowerCase())) {
                showFieldError('registerEmail', 'Cette adresse email est déjà utilisée');
                reject(new Error('Email déjà utilisé'));
                return;
            }

            const rgpdConsent = document.getElementById('rgpdConsent');
            if (!rgpdConsent || !rgpdConsent.checked) {
                showMessage('error', 'Vous devez accepter la politique de confidentialité.');
                reject(new Error('RGPD non accepté'));
                return;
            }

            // ⭐ Récupération des préférences newsletter
            const newsletterConsent = document.getElementById('newsletterConsent')?.checked || false;
            const newsletterPreferences = {
                subscribed: newsletterConsent,
                categories: {
                    sport: newsletterConsent && document.querySelector('[name="category_sport"]')?.checked || false,
                    evenements: newsletterConsent && document.querySelector('[name="category_evenements"]')?.checked || false,
                    billets: newsletterConsent && document.querySelector('[name="category_billets"]')?.checked || false
                },
                subscribedAt: newsletterConsent ? new Date().toISOString() : null,
                unsubscribeToken: newsletterConsent ? generateUnsubscribeToken() : null
            };

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
            
            currentUser = newUser;
            localStorage.setItem('jo_current_user', JSON.stringify(currentUser));

            // ⭐ Message personnalisé selon l'abonnement newsletter
            let successMsg = `Félicitations ${newUser.firstName} ! Votre compte a été créé avec succès. `;
            if (newsletterConsent) {
                successMsg += `📧 Vous recevrez un email de confirmation de votre abonnement à la newsletter.`;
            }
            successMsg += ` Vous êtes maintenant connecté(e).`;

            showMessage('success', successMsg);

            setTimeout(() => {
                window.location.href = 'index.html';
            }, 3000);

            resolve(newUser);
        }, 2000);
    });
}

// Gestion du mot de passe oublié
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

            const resetToken = generateResetToken();
            user.resetToken = resetToken;
            user.resetTokenExpiry = new Date(Date.now() + 3600000).toISOString();
            updateUserInStorage(user);

            showMessage('success', 
                `Un lien de réinitialisation a été envoyé à ${email}. ` +
                `Veuillez vérifier votre boîte de réception et vos spams. ` +
                `Le lien expire dans 1 heure.`
            );

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

// Fonctions utilitaires
function generateUserId() {
    return 'user_' + Date.now() + '_' + Math.random().toString(36).substr(2, 9);
}

function generateSecurityKey() {
    return 'sk_' + Math.random().toString(36).substr(2, 20) + Date.now().toString(36);
}

function generateResetToken() {
    return 'rst_' + Math.random().toString(36).substr(2, 32);
}

// ⭐ NOUVEAU: Génération du token de désinscription newsletter
function generateUnsubscribeToken() {
    return 'unsubscribe_' + Math.random().toString(36).substr(2, 32) + Date.now();
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

function showLoading() {
    const loadingSection = document.getElementById('loadingSection');
    if (loadingSection) {
        loadingSection.style.display = 'block';
    }
    
    document.querySelectorAll('button[type="submit"]').forEach(btn => {
        btn.disabled = true;
    });
}

function hideLoading() {
    const loadingSection = document.getElementById('loadingSection');
    if (loadingSection) {
        loadingSection.style.display = 'none';
    }
    
    document.querySelectorAll('button[type="submit"]').forEach(btn => {
        btn.disabled = false;
    });
}

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

function initializeInputValidation() {
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
        
        if (input.type === 'email') {
            input.addEventListener('blur', () => {
                const email = input.value.trim();
                if (email && !validateEmail(email)) {
                    showFieldError(input.id, 'Format d\'email invalide');
                }
            });
        }
    });
    
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

function logout() {
    currentUser = null;
    localStorage.removeItem('jo_current_user');
    window.location.href = 'connexion.html';
}

window.JO_Auth = {
    getCurrentUser: () => currentUser,
    logout,
    isLoggedIn: () => currentUser !== null
};