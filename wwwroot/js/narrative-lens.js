document.addEventListener('DOMContentLoaded', function () {
    const carousel = document.querySelector('.lens-carousel');
    const slides = document.querySelectorAll('.lens-slide');
    const indicators = document.querySelectorAll('.lens-indicator');
    const prevBtn = document.querySelector('.lens-prev-btn');
    const nextBtn = document.querySelector('.lens-next-btn');

    if (!carousel || slides.length === 0) return;

    // Variabili di stato
    let currentIndex = 0;
    const slideCount = slides.length;
    let isAnimating = false;
    let autoplayTimer;
    let touchStartX = 0;
    let touchEndX = 0;

    // Inizializzazione
    initCarousel();

    function initCarousel() {
        // Assegna indici data-index ai paragrafi per animazione sfalsata
        slides.forEach(slide => {
            const paragraphs = slide.querySelectorAll('.lens-article p');
            paragraphs.forEach((p, i) => {
                p.style.setProperty('--i', i + 1);
            });
        });

        // Inizializza lo stato del carousel
        updateCarousel(true);

        // Avvia autoplay se ci sono più slide
        if (slideCount > 1) {
            startAutoplay();
        }
    }

    

    // Event listeners
    indicators.forEach((indicator, index) => {
        indicator.addEventListener('click', () => {
            if (isAnimating || currentIndex === index) return;
            goToSlide(index);
        });

        // Aggiungi animazione hover
        indicator.addEventListener('mouseenter', () => {
            const emoji = indicator.querySelector('.lens-emoji');
            if (emoji) {
                emoji.style.transform = 'scale(1.3) rotate(5deg)';
            }
        });

        indicator.addEventListener('mouseleave', () => {
            const emoji = indicator.querySelector('.lens-emoji');
            if (emoji) {
                emoji.style.transform = '';
            }
        });
    });

    prevBtn.addEventListener('click', () => {
        if (isAnimating) return;
        goToSlide((currentIndex - 1 + slideCount) % slideCount);
    });

    nextBtn.addEventListener('click', () => {
        if (isAnimating) return;
        goToSlide((currentIndex + 1) % slideCount);
    });

    // Support for keyboard navigation
    document.addEventListener('keydown', (e) => {
        if (isAnimating) return;

        if (e.key === 'ArrowLeft') {
            goToSlide((currentIndex - 1 + slideCount) % slideCount);
        } else if (e.key === 'ArrowRight') {
            goToSlide((currentIndex + 1) % slideCount);
        }
    });

    // Support for touch swipe
    carousel.addEventListener('touchstart', (e) => {
        touchStartX = e.changedTouches[0].screenX;

        // Pausa autoplay temporaneamente quando l'utente interagisce
        
    }, { passive: true });

    carousel.addEventListener('touchend', (e) => {
        touchEndX = e.changedTouches[0].screenX;
        handleSwipe();

        // Riavvia autoplay dopo l'interazione
        
    }, { passive: true });

    function handleSwipe() {
        if (isAnimating) return;

        const threshold = 50;
        if (touchEndX < touchStartX - threshold) {
            // Swipe left
            goToSlide((currentIndex + 1) % slideCount);
        } else if (touchEndX > touchStartX + threshold) {
            // Swipe right
            goToSlide((currentIndex - 1 + slideCount) % slideCount);
        }
    }

    function goToSlide(index, isAuto = false) {
        if (index === currentIndex) return;

        isAnimating = true;

        // Gestisci la direzione dell'animazione
        const direction = index > currentIndex ? 'next' : 'prev';
        const currentSlide = slides[currentIndex];
        const nextSlide = slides[index];

        // Prepara l'animazione
        currentSlide.style.zIndex = 2;
        nextSlide.style.zIndex = 1;

        // Animazione in uscita per la slide corrente
        currentSlide.classList.add(direction === 'next' ? 'exit-to-left' : 'exit-to-right');

        // Aggiorna lo stato
        currentIndex = index;

        // Aggiorna UI
        updateCarousel();

        // Completa l'animazione
        setTimeout(() => {
            currentSlide.classList.remove('active', 'exit-to-left', 'exit-to-right');
            nextSlide.classList.add('active');
            isAnimating = false;
        }, 500);

        // Se non è un cambio automatico, resetta l'autoplay
        if (!isAuto) {
            startAutoplay();
        }
    }

    function updateCarousel(isInitial = false) {
        // Aggiorna la posizione delle slide
        const translateX = -currentIndex * 100;
        carousel.style.transform = `translateX(${translateX}%)`;

        // Aggiorna gli indicatori
        indicators.forEach((indicator, index) => {
            if (index === currentIndex) {
                indicator.classList.add('active');
            } else {
                indicator.classList.remove('active');
            }
        });

        // Aggiorna pulsanti di navigazione
        prevBtn.disabled = slideCount <= 1;
        nextBtn.disabled = slideCount <= 1;

        // Aggiungi classe specifica alla prospettiva al body
        const currentSlide = slides[currentIndex];
        const perspectiveType = currentSlide.dataset.perspectiveType;

        document.body.className = document.body.className.replace(/perspective-\w+/g, '');
        document.body.classList.add(`perspective-${perspectiveType}`);

        // Imposta l'active state alle slide
        if (isInitial) {
            slides.forEach((slide, index) => {
                if (index === currentIndex) {
                    slide.classList.add('active');
                } else {
                    slide.classList.remove('active');
                }
            });
        }
    }

    // Gestione condivisione
    const shareBtn = document.querySelector('.share-btn');
    if (shareBtn) {
        shareBtn.addEventListener('click', async () => {
            try {
                const title = document.title;
                const url = window.location.href;
                const slideType = slides[currentIndex]?.dataset.perspectiveType || '';
                const slideName = indicators[currentIndex]?.querySelector('.lens-name')?.textContent || '';

                const shareText = `Esplora la prospettiva di ${slideName} su PrismaNews: ${title}`;

                if (navigator.share) {
                    await navigator.share({
                        title: title,
                        text: shareText,
                        url: url
                    });
                } else {
                    // Fallback
                    navigator.clipboard.writeText(`${shareText} ${url}`);

                    const tooltip = document.createElement('div');
                    tooltip.className = 'share-tooltip';
                    tooltip.textContent = 'Link copiato negli appunti!';

                    // Rimuovi tooltip precedenti
                    const oldTooltips = document.querySelectorAll('.share-tooltip');
                    oldTooltips.forEach(t => t.remove());

                    shareBtn.appendChild(tooltip);

                    setTimeout(() => {
                        tooltip.style.opacity = 0;
                        setTimeout(() => tooltip.remove(), 300);
                    }, 2000);

                    // Effetto feedback visivo
                    shareBtn.classList.add('pulse-animation');
                    setTimeout(() => shareBtn.classList.remove('pulse-animation'), 1000);
                }
            } catch (error) {
                console.error('Error sharing:', error);
            }
        });
    }

    // Effetto parallasse avanzato
    const lensHeaders = document.querySelectorAll('.lens-content .lens-header');
    if (lensHeaders.length > 0) {
        window.addEventListener('scroll', () => {
            const scrollTop = window.pageYOffset || document.documentElement.scrollTop;

            lensHeaders.forEach(header => {
                // Calcola la posizione dell'elemento rispetto alla viewport
                const rect = header.getBoundingClientRect();

                // Applica l'effetto solo se l'elemento è visibile
                if (rect.bottom > 0 && rect.top < window.innerHeight) {
                    const speed = 0.4;
                    const yPos = (scrollTop * speed);
                    header.style.backgroundPositionY = `calc(50% + ${yPos}px)`;
                }
            });
        }, { passive: true });
    }

    // Anima i keywords all'hover
    const keywords = document.querySelectorAll('.lens-keyword');
    keywords.forEach(keyword => {
        keyword.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-4px)';
        });

        keyword.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });
    });

    // Gestisci visibilità della pagina per pause autoplay
    document.addEventListener("visibilitychange", () => {
        if (document.hidden) {
            clearTimeout(autoplayTimer);
        } else {
            startAutoplay();
        }
    });

    // Animazione del titolo principale
    const mainTitle = document.querySelector('.lens-header h1');
    if (mainTitle) {
        mainTitle.addEventListener('mouseenter', function () {
            this.style.transform = 'scale(1.03)';
        });

        mainTitle.addEventListener('mouseleave', function () {
            this.style.transform = '';
        });
    }

    // Aggiungi dinamicamente namespace CSS per gli stili specifici per ogni prospettiva
    function addPerspectiveStyles() {
        const styleEl = document.createElement('style');
        styleEl.textContent = `
            /* Stili dinamici per le prospettive */
            .perspective-activist { --current-color: var(--activist-primary); }
            .perspective-politician { --current-color: var(--politician-primary); }
            .perspective-expert { --current-color: var(--expert-primary); }
            .perspective-victim { --current-color: var(--victim-primary); }
            
            /* Animazioni per uscita slide */
            .lens-slide.exit-to-left {
                animation: exitToLeft 0.5s forwards;
            }
            
            .lens-slide.exit-to-right {
                animation: exitToRight 0.5s forwards;
            }
            
            @keyframes exitToLeft {
                from { transform: translateX(0) scale(1); opacity: 1; }
                to { transform: translateX(-30px) scale(0.9); opacity: 0; }
            }
            
            @keyframes exitToRight {
                from { transform: translateX(0) scale(1); opacity: 1; }
                to { transform: translateX(30px) scale(0.9); opacity: 0; }
            }
            
            /* Animazione pulse per bottone share */
            .pulse-animation {
                animation: pulse-btn 1s cubic-bezier(0.25, 0.46, 0.45, 0.94);
            }
            
            @keyframes pulse-btn {
                0% { transform: scale(1); }
                50% { transform: scale(1.1); }
                100% { transform: scale(1); }
            }
        `;
        document.head.appendChild(styleEl);
    }

    // Inizializza stili dinamici
    addPerspectiveStyles();
});