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


# Mise à jour de branch distante
git push -u origin dev


# Creation du backend
mkdir backend
cd backend
dotnet new webapi -n JO2024API

# Lier le repository
git remote add origin https://github.com/SandrineC34/JO_2024_PARIS.git

# Etape n°1 déploiement de l'application statique uniquement
render 
Build Command : npm install
Start Command : npm start


lancer le test


dotnet clean
dotnet restore
dotnet build JeuxOlympiques.csproj -c Release