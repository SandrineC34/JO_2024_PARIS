// frontend/js/config.js
// Configuration globale pour le frontend

const CONFIG = {
    // URL de l'API - Change automatiquement selon l'environnement
    API_BASE_URL: window.location.hostname === 'localhost' || window.location.hostname === '127.0.0.1'
        ? 'http://localhost:5000/api'  // Développement local
        : '/api',  // Production (via proxy Nginx)
    
    // Autres configurations
    ITEMS_PER_PAGE: 20,
    TOKEN_KEY: 'jo2024_token',
    USER_KEY: 'jo2024_user'
};

// Fonction utilitaire pour les appels API
async function apiCall(endpoint, options = {}) {
    const token = localStorage.getItem(CONFIG.TOKEN_KEY);
    
    const defaultOptions = {
        headers: {
            'Content-Type': 'application/json',
            ...(token && { 'Authorization': `Bearer ${token}` })
        }
    };
    
    const finalOptions = {
        ...defaultOptions,
        ...options,
        headers: {
            ...defaultOptions.headers,
            ...options.headers
        }
    };
    
    try {
        const response = await fetch(`${CONFIG.API_BASE_URL}${endpoint}`, finalOptions);
        
        // Gérer l'expiration du token
        if (response.status === 401) {
            localStorage.removeItem(CONFIG.TOKEN_KEY);
            localStorage.removeItem(CONFIG.USER_KEY);
            if (!window.location.pathname.includes('connexion.html')) {
                window.location.href = '/html/connexion.html';
            }
            throw new Error('Session expirée');
        }
        
        if (!response.ok) {
            const error = await response.json();
            throw new Error(error.message || 'Erreur API');
        }
        
        return await response.json();
    } catch (error) {
        console.error('API Error:', error);
        throw error;
    }
}

// Exemples d'utilisation dans vos fichiers JS existants:

// Connexion
async function login(email, password) {
    const data = await apiCall('/auth/login', {
        method: 'POST',
        body: JSON.stringify({ email, password })
    });
    
    localStorage.setItem(CONFIG.TOKEN_KEY, data.token);
    localStorage.setItem(CONFIG.USER_KEY, JSON.stringify(data.user));
    return data;
}

// Récupérer les offres
async function getOffres() {
    return await apiCall('/offres');
}

// Créer une commande
async function createCommande(items) {
    return await apiCall('/commandes', {
        method: 'POST',
        body: JSON.stringify({ items })
    });
}

// Récupérer mes billets
async function getMyBillets() {
    return await apiCall('/billets');
}

// Export pour utilisation globale
window.API = {
    call: apiCall,
    login,
    getOffres,
    createCommande,
    getMyBillets,
    CONFIG
};