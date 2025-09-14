# Installation de l'extension GitHub Repositories
github Reositories
Github CodesSpaces
Github Actions

# Creation du projet du repro GIT
git init
echo "node_modules/" > .gitgnore
git add .
git commit -m "Initial Commit static page web"

# Creation mise à jour de la branch main
git checkout main
git pull
git merge dev
git push

# Mise à jour de branch distante
git push -u origin dev

# Lier le repository
git remote add origin https://github.com/SandrineC34/JO_2024_PARIS.git

# Etape n°1 déploiement de l'application statique uniquement
render 
Build Command : npm install
Start Command : npm start