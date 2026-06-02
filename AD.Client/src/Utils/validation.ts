
export function validatePassword(
    password: string,
    username: string,
    firstName: string,
    lastName: string
): string | null {
    if (!password) {
        return "Adgangskode skal udfyldes.";
    }

    
    if (password.length < 8) {
        return "Adgangskoden skal være på mindst 8 tegn.";
    }

    
    let categoriesPassed = 0;
    if (/[A-Z]/.test(password)) categoriesPassed++;
    if (/[a-z]/.test(password)) categoriesPassed++;
    if (/[0-9]/.test(password)) categoriesPassed++;
    if (/[^A-Za-z0-9]/.test(password)) categoriesPassed++;

    if (categoriesPassed < 3) {
        return "Adgangskoden skal indeholde tegn fra mindst 3 af følgende kategorier:\n" +
               "- Store bogstaver (A-Z)\n" +
               "- Små bogstaver (a-z)\n" +
               "- Tal (0-9)\n" +
               "- Specialtegn (f.eks. !, $, #, %)";
    }

    
    
    const hasNamePart = (name: string): boolean => {
        const cleanName = (name ?? "").toLowerCase().trim();
        if (cleanName.length < 3) return false;
        
        const cleanPassword = password.toLowerCase();
        for (let i = 0; i <= cleanName.length - 3; i++) {
            const part = cleanName.substring(i, i + 3);
            if (cleanPassword.includes(part)) {
                return true;
            }
        }
        return false;
    };

    if (hasNamePart(username)) {
        return "Adgangskoden må ikke indeholde dele af brugernavnet (3 eller flere sammenhængende tegn).";
    }
    if (hasNamePart(firstName)) {
        return "Adgangskoden må ikke indeholde dele af fornavnet (3 eller flere sammenhængende tegn).";
    }
    if (hasNamePart(lastName)) {
        return "Adgangskoden må ikke indeholde dele af efternavnet (3 eller flere sammenhængende tegn).";
    }

    return null;
}
