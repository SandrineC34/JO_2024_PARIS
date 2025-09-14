const express = require('express');
const path = require('path');
const app = express();

// Port dynamique pour Render
const PORT = process.env.PORT || 3000;

// Middleware pour servir les fichiers statiques
app.use(express.static(__dirname));

// Routes spécifiques pour vos pages HTML
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

app.get('/accueil', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

app.get('/offres', (req, res) => {
    res.sendFile(path.join(__dirname, 'offres.html'));
});

app.get('/panier', (req, res) => {
    res.sendFile(path.join(__dirname, 'panier.html'));
});

app.get('/compte', (req, res) => {
    res.sendFile(path.join(__dirname, 'compte.html'));
});

app.get('/connexion', (req, res) => {
    res.sendFile(path.join(__dirname, 'connexion.html'));
});

// Servir les fichiers CSS, JS, Images
app.get('/css/*', (req, res) => {
    res.sendFile(path.join(__dirname, req.path));
});

app.get('/js/*', (req, res) => {
    res.sendFile(path.join(__dirname, req.path));
});

app.get('/images/*', (req, res) => {
    res.sendFile(path.join(__dirname, req.path));
});

// Servir les fichiers header et footer pour vos appels AJAX
app.get('/header.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'header.html'));
});

app.get('/footer.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'footer.html'));
});

// Catch-all pour rediriger vers index.html (SPA behavior)
app.get('*', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

// Démarrer le serveur
app.listen(PORT, () => {
    console.log(`🚀 Serveur démarré sur le port ${PORT}`);
    console.log(`📱 Application disponible sur http://localhost:${PORT}`);
});
