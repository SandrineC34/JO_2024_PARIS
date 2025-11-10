// ✅ CORRECTIF pour initializeNewsletterToggle dans connexion.js
// Remplacez la fonction existante par celle-ci

function initializeNewsletterToggle() {
    const newsletterConsent = document.getElementById('newsletterConsent');
    const newsletterCategories = document.getElementById('newsletterCategories');

    if (newsletterConsent && newsletterCategories) {
        // ✅ Par défaut: NON coché (conforme RGPD opt-in)
        newsletterConsent.checked = false;
        newsletterCategories.style.display = 'none';
        
        // Désactiver les catégories par défaut
        document.querySelectorAll('[name^="category_"]').forEach(cb => {
            cb.checked = false;
            cb.disabled = true; // ✅ IMPORTANT: désactiver tant que newsletter non cochée
        });

        // ✅ NOUVEAU: Gérer le changement d'état de la checkbox principale
        newsletterConsent.addEventListener('change', function() {
            const isChecked = this.checked;
            
            // Afficher/masquer les catégories
            newsletterCategories.style.display = isChecked ? 'block' : 'none';
            
            // Activer/désactiver les checkboxes de catégories
            document.querySelectorAll('[name^="category_"]').forEach(cb => {
                cb.disabled = !isChecked; // ✅ Activer si newsletter cochée
                
                // Décocher si on décoche la newsletter
                if (!isChecked) {
                    cb.checked = false;
                }
            });
            
            console.log('Newsletter consent:', isChecked);
        });
        
        // ✅ NOUVEAU: Logger les changements de catégories
        document.querySelectorAll('[name^="category_"]').forEach(cb => {
            cb.addEventListener('change', function() {
                console.log(`Catégorie ${this.name}:`, this.checked);
            });
        });
    }
}

// ✅ MODIFICATION de simulateRegister pour mieux logger les données
// Trouvez cette partie dans votre connexion.js et modifiez-la :

// Dans la fonction simulateRegister, remplacez la partie récupération des préférences par :

// ✅ Récupération des préférences newsletter
const newsletterConsent = document.getElementById('newsletterConsent')?.checked || false;
const newsletterPreferences = {
    subscribed: newsletterConsent,
    categories: {
        sport: document.getElementById('category_sport')?.checked || false,
        evenements: document.getElementById('category_evenements')?.checked || false,
        billets: document.getElementById('category_billets')?.checked || false
    },
    subscribedAt: newsletterConsent ? new Date().toISOString() : null,
    unsubscribeToken: newsletterConsent ? generateUnsubscribeToken() : null
};

console.log('📧 Newsletter preferences:', newsletterPreferences);

// Le reste du code reste identique...