
// Données de test utilisateur
const userData = {
    firstName: 'Jean',
    lastName: 'Dupont',
    email: 'jean.dupont@email.com',
    memberSince: '2024-05-15'
};

// Menu mobile
function toggleMenu() {
    const navMenu = document.getElementById('navMenu');
    navMenu.classList.toggle('active');
}

// Navigation des sections du compte
function showSection(sectionId) {
    // Masquer toutes les sections
    document.querySelectorAll('.section').forEach(section => {
        section.classList.remove('active');
    });

    // Retirer la classe active de tous les liens
    document.querySelectorAll('.nav-link').forEach(link => {
        link.classList.remove('active');
    });

    // Afficher la section demandée
    document.getElementById(sectionId).classList.add('active');
    
    // Activer le lien correspondant
    document.querySelector(`[data-section="${sectionId}"]`).classList.add('active');
}

// Gestion des clics sur la navigation
document.querySelectorAll('.nav-link').forEach(link => {
    link.addEventListener('click', (e) => {
        e.preventDefault();
        const sectionId = link.getAttribute('data-section');
        showSection(sectionId);
    });
});

// Basculer les détails d'un billet
function toggleTicketDetails(ticketCard) {
    const details = ticketCard.querySelector('.ticket-details');
    const isExpanded = ticketCard.classList.contains('expanded');

    // Fermer tous les autres billets
    document.querySelectorAll('.ticket-card').forEach(card => {
        card.classList.remove('expanded');
        card.querySelector('.ticket-details').classList