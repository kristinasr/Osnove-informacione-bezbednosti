Projekat iz predmeta Osnove informacione bezbednosti. Servis pruza usluge koriscenja i upravljanja tekstualnom bazom podataka. Postoje tri tipa klijenata:
1. Admin
2. Reader
3. Writer
Svaki od klijenata ima razlicita ovlascenja. Autentifikacija je uradjena preko Windows autentifikacionog protokola, a autorizacija po RBAC semi.
Autentifikacija izmedju primarnog i sekundarnog servisa uradjena pomocu sertifikata, poruke izmedju njih se sifruju AES algoritmom.
