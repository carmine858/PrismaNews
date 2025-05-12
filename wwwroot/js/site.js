// Main JavaScript for Prisma News
document.addEventListener('DOMContentLoaded', function () {

    // Navbar scroll effect
    const navbar = document.querySelector('.navbar');
    window.addEventListener('scroll', function () {
        if (window.scrollY > 50) {
            navbar.classList.add('navbar-scrolled');
        } else {
            navbar.classList.remove('navbar-scrolled');
        }
    });

    // Initialize Bootstrap tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl)
    });

    // Theme switcher
    const themeSwitch = document.getElementById('themeSwitch');
    if (themeSwitch) {
        const lightIcon = document.querySelector('.light-icon');
        const darkIcon = document.querySelector('.dark-icon');

        // Check for saved theme preference
        const prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
        const savedTheme = localStorage.getItem('theme');

        if (savedTheme === 'dark' || (!savedTheme && prefersDark)) {
            document.body.classList.add('dark-theme');
            themeSwitch.checked = true;
            darkIcon.classList.add('active');
            lightIcon.classList.remove('active');
        } else {
            lightIcon.classList.add('active');
            darkIcon.classList.remove('active');
        }

        themeSwitch.addEventListener('change', function () {
            if (this.checked) {
                document.body.classList.add('dark-theme');
                localStorage.setItem('theme', 'dark');
                darkIcon.classList.add('active');
                lightIcon.classList.remove('active');
            } else {
                document.body.classList.remove('dark-theme');
                localStorage.setItem('theme', 'light');
                lightIcon.classList.add('active');
                darkIcon.classList.remove('active');
            }
        });
    }

    // Create animated particles in hero section
    createParticles();

    // Add scroll reveal animations
    initScrollReveal();

    // Initialize news card hover effects
    initCardHoverEffects();
});

// Create floating particles in the hero section
function createParticles() {
    const heroSection = document.querySelector('.hero-section');
    if (!heroSection) return;

    const particlesContainer = document.createElement('div');
    particlesContainer.className = 'particles-container';
    heroSection.appendChild(particlesContainer);

    // Add particles
    for (let i = 0; i < 20; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';

        // Random size between 5 and 20px
        const size = Math.random() * 15 + 5;
        particle.style.width = `${size}px`;
        particle.style.height = `${size}px`;

        // Random position
        const posX = Math.random() * 100;
        const posY = Math.random() * 100;
        particle.style.left = `${posX}%`;
        particle.style.top = `${posY}%`;

        // Random animation duration between 20-40s
        const duration = Math.random() * 20 + 20;
        particle.style.animation = `floatingParticle ${duration}s infinite alternate ease-in-out`;

        // Add custom animation delay
        particle.style.animationDelay = `${Math.random() * 5}s`;

        particlesContainer.appendChild(particle);
    }

    // Add keyframe animation dynamically
    const style = document.createElement('style');
    style.textContent = `
        @keyframes floatingParticle {
            0% { transform: translate(0, 0) rotate(0deg); opacity: 0.2; }
            50% { opacity: 0.5; }
            100% { transform: translate(${Math.random() > 0.5 ? '+' : '-'}${Math.random() * 100 + 50}px, ${Math.random() > 0.5 ? '+' : '-'}${Math.random() * 100 + 50}px) rotate(${Math.random() * 360}deg); opacity: 0.2; }
        }
    `;
    document.head.appendChild(style);
}

// Initialize scroll reveal animations
function initScrollReveal() {
    const sections = document.querySelectorAll('.featured-section, .polarizing-section, .recent-section, .how-it-works-section, .features-section, .cta-section');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('animate__animated', 'animate__fadeIn');
                observer.unobserve(entry.target);
            }
        });
    }, {
        threshold: 0.1
    });

    sections.forEach(section => {
        observer.observe(section);
    });
}

// Initialize card hover effects
function initCardHoverEffects() {
    const cards = document.querySelectorAll('.news-card, .featured-news-card, .how-it-works-card');

    cards.forEach(card => {
        // Add tilt effect
        card.addEventListener('mousemove', function (e) {
            const rect = this.getBoundingClientRect();
            const x = e.clientX - rect.left;
            const y = e.clientY - rect.top;

            const centerX = rect.width / 2;
            const centerY = rect.height / 2;

            const angleX = (y - centerY) / 20;
            const angleY = (centerX - x) / 20;

            this.style.transform = `perspective(1000px) rotateX(${angleX}deg) rotateY(${angleY}deg) translateY(-10px) scale(1.03)`;
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });
    });
}

// Show notifications
function showNotification(message, type = 'success', duration = 5000) {
    const toastContainer = document.createElement('div');
    toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
    toastContainer.style.zIndex = '9999';

    const toast = document.createElement('div');
    toast.className = `toast align-items-center text-white bg-${type} border-0 animate__animated animate__fadeInRight`;
    toast.setAttribute('role', 'alert');
    toast.setAttribute('aria-live', 'assertive');
    toast.setAttribute('aria-atomic', 'true');

    toast.innerHTML = `
        <div class="d-flex">
            <div class="toast-body">
                ${message}
            </div>
            <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button>
        </div>
    `;

    toastContainer.appendChild(toast);
    document.body.appendChild(toastContainer);

    const bsToast = new bootstrap.Toast(toast, { delay: duration });
    bsToast.show();

    toast.addEventListener('hidden.bs.toast', function () {
        toastContainer.remove();
    });
}

// Smooth scroll function
function scrollToElement(elementId) {
    const element = document.getElementById(elementId);
    if (element) {
        window.scrollTo({
            top: element.offsetTop - 100,
            behavior: 'smooth'
        });
    }
}
document.addEventListener("DOMContentLoaded", function () {
    // Forzare la clickabilità di tutti i bottoni nella homepage
    const buttons = document.querySelectorAll('.btn, button, a.btn, [role="button"]');
    buttons.forEach(button => {
        // Rimuovi eventuali event listener esistenti
        const newButton = button.cloneNode(true);
        button.parentNode.replaceChild(newButton, button);

        // Aggiungi un event listener esplicito
        if (newButton.tagName === 'A' && newButton.hasAttribute('href')) {
            newButton.addEventListener('click', function (e) {
                const href = this.getAttribute('href');
                if (href && href !== '#' && href !== 'javascript:void(0)') {
                    window.location.href = href;
                }
            });
        }
    });
});
// Toggle password visibility
function togglePasswordVisibility(inputId, icon) {
    const passwordInput = document.getElementById(inputId);
    if (!passwordInput) return;

    const iconElement = icon.querySelector('i');

    if (passwordInput.type === 'password') {
        passwordInput.type = 'text';
        iconElement.classList.remove('bi-eye');
        iconElement.classList.add('bi-eye-slash');
    } else {
        passwordInput.type = 'password';
        iconElement.classList.remove('bi-eye-slash');
        iconElement.classList.add('bi-eye');
    }
}