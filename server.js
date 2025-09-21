const express = require('express');
const path = require('path');
const app = express();

// Port pour Render
const PORT = process.env.PORT || 3000;

// Servir tous les fichiers statiques depuis le dossier frontend
app.use(express.static(path.join(__dirname, 'frontend')));

// Route principale - servir index.html depuis frontend/html/
app.get('/', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/index.html'));
});

// Routes pour pages HTML depuis frontend/html/
app.get('/offres.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/offres.html'));
});

app.get('/offres', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/offres.html'));
});

app.get('/panier.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/panier.html'));
});

app.get('/panier', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/panier.html'));
});

app.get('/compte.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/compte.html'));
});

app.get('/compte', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/compte.html'));
});

app.get('/connexion.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/connexion.html'));
});

app.get('/connexion', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/connexion.html'));
});

// Routes pour header et footer (pour vos appels AJAX)
app.get('/header.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/header.html'));
});

app.get('/footer.html', (req, res) => {
    res.sendFile(path.join(__dirname, 'frontend/html/footer.html'));
});

// Route catch-all pour les fichiers statiques manqués
app.get('*', (req, res) => {
    // Essayer de servir le fichier demandé depuis frontend
    const filePath = path.join(__dirname, 'frontend', req.path);
    res.sendFile(filePath, (err) => {
        if (err) {
            // Si le fichier n'existe pas, rediriger vers index
            res.sendFile(path.join(__dirname, 'frontend/html/index.html'));
        }
    });
});

// Démarrage du serveur
app.listen(PORT, '0.0.0.0', () => {
    console.log(` Serveur démarré sur le port ${PORT}`);
    console.log(` Application accessible sur http://localhost:${PORT}`);
});