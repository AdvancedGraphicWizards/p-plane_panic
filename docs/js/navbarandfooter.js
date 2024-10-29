async function loadNavbarAndFooter() {
    try {
        const navbarResponse = await fetch('html/navbar.html');
        const navbarHTML = await navbarResponse.text();
        document.getElementById('navbar-container').innerHTML = navbarHTML;

        const footerResponse = await fetch('html/footer.html');
        const footerHTML = await footerResponse.text();
        document.getElementById('footer-container').innerHTML = footerHTML;

        // Highlight the active page in the navbar, get the current file name
        const currentPath = window.location.pathname.split("/").pop();
        const navLinks = document.querySelectorAll('.nav-link');
        navLinks.forEach(link => {
            if (link.getAttribute('href') === currentPath) {
                link.classList.add('active');
            }
        });
    } catch (error) {
        console.error('Error loading navbar or footer:', error);
    }
}

document.addEventListener('DOMContentLoaded', loadNavbarAndFooter);