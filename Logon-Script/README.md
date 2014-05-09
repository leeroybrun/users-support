This script will update the current computer's description in AD with the current logged in user and the date/time.

1. Add "userLogon.vbs" to the logon scripts of each computers (GPO is good for that)
2. Add the rights for users to edit computers' description in AD
   2.1 Right click on AD main tree
   2.2 Délégation de contrôle
   2.3 Add the users' groups -> Next
   2.4 Créer une tâche personnalisée -> Next
   2.5 Seulement aux objets suivants : Objets Ordinateur -> Next
   2.6 Cocher : "Lire" et "Ecrire Description" -> Next
   2.7 Terminer