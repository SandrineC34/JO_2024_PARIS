# Pour faire une inscription rapidement

curl -X POST http://localhost:5000/api/Auth/register \
  -H "Content-Type: application/json" \
  -d '{
      "Prenom": "robert",
      "Nom": "Deniro",
      "Email": "deniro.do@test.com",
      "Password": "TestPass123!",
      "NewsletterPreferences": {
        "Subscribed": true,
        "Categories": {
          "Sport": true,
          "Evenements": false,
          "Billets": false
        },
        "Sports": [
          {
            "Id": "natation",
            "Name": "Natation"
          }
        ]
      },
      "PrivacyPolicyAccepted": true  
  }'
