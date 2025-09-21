const express = require('express');
const path = require('path');
const app = express();

// Port pour Render
const PORT = process.env.PORT || 3000;

// Servir tous les fichiers statiques (HTML, CSS, JS, images)
app.use(express.static(__dirname));

// Route principale - servir index.html
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'index.html'));
});

// Routes pour pages HTML (gardent leurs extensions)
app.get('/offres.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'offres.html'));
});

app.get('/panier.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'panier.html'));
});

app.get('/compte.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'compte.html'));
});

app.get('/connexion.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'connexion.html'));
});

// Routes pour header et footer (pour vos appels AJAX)
app.get('/header.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'header.html'));
});

app.get('/footer.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'footer.html'));
});

// Démarrage du serveur
app.listen(PORT, '0.0.0.0', () => {
    console.log(` Serveur démarré sur le port ${PORT}`);
    console.log(` Application accessible sur http://localhost:${PORT}`);
});