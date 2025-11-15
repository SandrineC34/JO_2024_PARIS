// connexion.js - Version corrigée avec debug

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
   console.log('🚀 Initialisation de la page connexion');
   initializeAuth();
   initializeNewsletterToggle();
   initializeInputValidation();
});

// Initialisation de l'authentification
function initializeAuth() {
    hideLoading();
    setTimeout(addEntryAnimation, 100);
}

// INITIALISATION NEWSLETTER AVEC DEBUG
function initializeNewsletterToggle() {
    console.log('📧 Initialisation newsletter toggle...');
    
    const newsletterConsent = document.getElementById('newsletterConsent');
    const newsletterCategories = document.getElementById('newsletterCategories');

    console.log('newsletterConsent trouvé:', !!newsletterConsent);
    console.log('newsletterCategories trouvé:', !!newsletterCategories);

    if (newsletterConsent && newsletterCategories) {
        // Par défaut: NON coché et masqué
        newsletterConsent.checked = false;
        newsletterCategories.style.display = 'none';
        
        console.log('✅ Newsletter initialisée (décochée par défaut)');

        // Gérer le changement d'état de la newsletter principale
        newsletterConsent.addEventListener('change', function() {
            console.log('📬 Newsletter changée:', this.checked);
            
            if (this.checked) {
                newsletterCategories.style.display = 'block';
                console.log('✅ Catégories affichées');
            } else {
                newsletterCategories.style.display = 'none';
                console.log('❌ Catégories masquées');
                
                // Décocher toutes les catégories
                document.querySelectorAll('[name^="category_"]').forEach(cb => {
                    cb.checked = false;
                });
                
                // Masquer et décocher les sports
                const sportsSubcategories = document.getElementById('sportsSubcategories');
                if (sportsSubcategories) {
                    sportsSubcategories.style.display = 'none';
                }
                
                document.querySelectorAll('[name^="sport_"]').forEach(cb => {
                    cb.checked = false;
                });
            }
        });
    } else {
        console.error('❌ Éléments newsletter introuvables!');
    }
    
    // Initialiser le toggle des sports
    initializeSportsToggle();
}

// ⭐ GESTION DES SPORTS 
function initializeSportsToggle() {
    console.log('⚽ Initialisation sports toggle...');
    
    const categorySport = document.getElementById('category_sport');
    const sportsSubcategories = document.getElementById('sportsSubcategories');
    
    console.log('category_sport trouvé:', !!categorySport);
    console.log('sportsSubcategories trouvé:', !!sportsSubcategories);
    
    if (!categorySport || !sportsSubcategories) {
        console.error('❌ Éléments sports introuvables!');
        console.log('Tous les éléments avec ID:', 
            Array.from(document.querySelectorAll('[id]')).map(el => el.id)
        );
        return;
    }
    
    // Masquer par défaut
    sportsSubcategories.style.display = 'none';
    console.log('✅ Sports masqués par défaut');
    
    // Gérer le changement d'état de la catégorie Sport
    categorySport.addEventListener('change', function() {
        console.log('🏅 Catégorie Sport changée:', this.checked);
        
        if (this.checked) {
            sportsSubcategories.style.display = 'block';
            console.log('✅ Liste des sports AFFICHÉE');
        } else {
            sportsSubcategories.style.display = 'none';
            console.log('❌ Liste des sports MASQUÉE');
            
            // Décocher tous les sports
            const sportCheckboxes = document.querySelectorAll('[name^="sport_"]');
            console.log(`Nombre de sports à décocher: ${sportCheckboxes.length}`);
            
            sportCheckboxes.forEach(checkbox => {
                checkbox.checked = false;
            });
        }
    });
    
    console.log('✅ Sports toggle initialisé avec succès');
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
    
    // Réinitialiser l'état de la newsletter
    const newsletterConsent = document.getElementById('newsletterConsent');
    const newsletterCategories = document.getElementById('newsletterCategories');
    const sportsSubcategories = document.getElementById('sportsSubcategories');
    
    if (newsletterConsent) newsletterConsent.checked = false;
    if (newsletterCategories) newsletterCategories.style.display = 'none';
    if (sportsSubcategories) sportsSubcategories.style.display = 'none';
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

// Gestion de l'inscription
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

    const rgpdConsent = document.getElementById('rgpdConsent');
    if (rgpdConsent && !rgpdConsent.checked) {
        showMessage('error', '⚠️ Vous devez accepter la politique de confidentialité pour créer un compte.');
        hasErrors = true;
    }

    return !hasErrors;
}

async function simulateRegister(firstName, lastName, email, password) {
    return new Promise((resolve, reject) => {
        setTimeout(() => {
            if (users.some(u => u.email.toLowerCase() === email.toLowerCase())) {
                showFieldError('registerEmail', 'Cette adresse email est déjà utilisée');
                hideLoading();
                reject(new Error('Email déjà utilisé'));
                return;
            }

            const rgpdConsent = document.getElementById('rgpdConsent');
            if (!rgpdConsent || !rgpdConsent.checked) {
                showMessage('error', 'Vous devez accepter la politique de confidentialité.');
                hideLoading();
                reject(new Error('RGPD non accepté'));
                return;
            }

            // Récupération des préférences newsletter
            const newsletterConsent = document.getElementById('newsletterConsent')?.checked || false;
            
            let selectedSports = [];
            if (newsletterConsent && document.getElementById('category_sport')?.checked) {
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
            
            const newsletterPreferences = {
                subscribed: newsletterConsent,
                categories: {
                    sport: newsletterConsent && (document.getElementById('category_sport')?.checked || false),
                    evenements: newsletterConsent && (document.getElementById('category_evenements')?.checked || false),
                    billets: newsletterConsent && (document.getElementById('category_billets')?.checked || false)
                },
                sports: selectedSports,
                subscribedAt: newsletterConsent ? new Date().toISOString() : null,
                unsubscribeToken: newsletterConsent ? generateUnsubscribeToken() : null
            };

            console.log('📧 Préférences newsletter:', newsletterPreferences);

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
                lastLogin: new Date().toISOString(),
                isActive: true
            };

            users.push(newUser);
            localStorage.setItem('jo_users', JSON.stringify(users));
            
            currentUser = newUser;
            localStorage.setItem('jo_current_user', JSON.stringify(currentUser));

            let successMsg = `Félicitations ${newUser.firstName} ! Votre compte a été créé avec succès. `;
            if (newsletterConsent) {
                successMsg += `📧 Vous êtes abonné à la newsletter`;
                if (selectedSports.length > 0) {
                    const sportsNames = selectedSports.map(s => s.name).join(', ');
                    successMsg += ` (Sports: ${sportsNames})`;
                }
                successMsg += '. ';
            }
            successMsg += `Redirection en cours...`;

            showMessage('success', successMsg);

            setTimeout(() => {
                hideLoading();
                window.location.href = 'compte.html';
            }, 2000);

            resolve(newUser);
        }, 1500);
    });
}

// Mot de passe oublié
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

// API
async function apiLogin(email, password) {
    const response = await fetch(`${CONFIG.apiBaseUrl}/auth/login`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ email, password })
    });
    
    const data = await response.json();
    if (!response.ok) throw new Error(data.message || 'Erreur de connexion');
    return data;
}

async function apiRegister(firstName, lastName, email, password) {
    const response = await fetch(`${CONFIG.apiBaseUrl}/auth/register`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ firstName, lastName, email, password })
    });
    
    const data = await response.json();
    if (!response.ok) throw new Error(data.message || 'Erreur lors de l\'inscription');
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