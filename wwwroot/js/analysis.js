// Funzioni per migliorare l'interattività della pagina di analisi
document.addEventListener('DOMContentLoaded', function () {

    // Inizializza i tooltip di Bootstrap
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'))
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl, {
            delay: { show: 50, hide: 100 }
        })
    });

    // Animazioni all'entrata per gli elementi principali
    animateElementsOnScroll();

    // Gestione dello switch delle tabs
    document.querySelectorAll('.nav-link').forEach(tab => {
        tab.addEventListener('click', function (e) {
            // Già gestito da Bootstrap, ma aggiungiamo animazioni personalizzate
            setTimeout(() => {
                const targetPane = document.querySelector(this.getAttribute('data-bs-target'));
                if (targetPane) {
                    animateEntrance(targetPane);
                }
            }, 150);
        });
    });

    // Pulsanti "Mostra di più"
    // Soluzione alternativa per pulsanti "Mostra tutti"
    document.querySelectorAll('.show-more-btn').forEach(btn => {
        btn.addEventListener('click', function (e) {
            e.preventDefault();

            // Determina quale tab aprire in base al contesto del pulsante
            let tabId = '';

            // Determina il tab target in base al contesto del pulsante
            if (this.closest('.bias-overview')) {
                tabId = 'bias-tab';
            } else if (this.closest('.rhetoric-overview')) {
                tabId = 'rhetoric-tab';
            } else {
                // Estrai dall'attributo se possibile
                const target = this.getAttribute('data-bs-target');
                if (target) {
                    tabId = target.replace('#', '') + '-tab';
                }
            }

            // Trova e attiva il tab
            if (tabId) {
                const tabElement = document.getElementById(tabId);
                if (tabElement) {
                    tabElement.click();
                }
            }
        });
    });

    // Gestione condivisione
    window.shareAnalysis = function (platform) {
        const url = window.location.href;
        const title = document.title;

        switch (platform) {
            case 'twitter':
                window.open(`https://twitter.com/intent/tweet?text=${encodeURIComponent(title)}&url=${encodeURIComponent(url)}`, '_blank');
                break;
            case 'facebook':
                window.open(`https://www.facebook.com/sharer/sharer.php?u=${encodeURIComponent(url)}`, '_blank');
                break;
            case 'whatsapp':
                window.open(`https://api.whatsapp.com/send?text=${encodeURIComponent(title + ' ' + url)}`, '_blank');
                break;
            case 'copy':
                navigator.clipboard.writeText(url).then(() => {
                    showToast('Link copiato negli appunti!');
                });
                break;
        }
    };

    // Pulsante Salva Analisi
    const saveBtn = document.getElementById('saveAnalysisBtn');
    if (saveBtn) {
        saveBtn.addEventListener('click', function () {
            // In un'app reale, qui salveresti l'analisi
            showToast('Analisi salvata nei preferiti!');
        });
    }

    // Effetto interattivo per i nodi del radar dei bias
    document.querySelectorAll('.bias-node').forEach(node => {
        node.addEventListener('mouseenter', function () {
            this.style.transform = `${this.style.transform.split(')')[0]}) scale(1.2)`;
            const label = this.querySelector('.node-label');
            if (label) {
                label.style.fontWeight = 'bold';
            }
        });

        node.addEventListener('mouseleave', function () {
            this.style.transform = this.style.transform.replace(' scale(1.2)', '');
            const label = this.querySelector('.node-label');
            if (label) {
                label.style.fontWeight = '';
            }
        });
    });

    // Inizializza il contatore per l'effetto di conteggio
    initCounters();
});

// Funzione per mostrare un toast di notifica
function showToast(message) {
    const toast = new bootstrap.Toast(document.createElement('div'));
    const toastContainer = document.createElement('div');
    toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
    toastContainer.style.zIndex = '11';

    const toastElement = document.createElement('div');
    toastElement.className = 'toast show';
    toastElement.role = 'alert';
    toastElement.innerHTML = `
        <div class="toast-header">
            <i class="bi bi-info-circle me-2"></i>
            <strong class="me-auto">Prisma News</strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">${message}</div>
    `;

    toastContainer.appendChild(toastElement);
    document.body.appendChild(toastContainer);

    setTimeout(() => {
        toastElement.classList.remove('show');
        setTimeout(() => {
            document.body.removeChild(toastContainer);
        }, 500);
    }, 3000);
}

// Effetto di entrata animata per gli elementi
function animateElementsOnScroll() {
    const elements = document.querySelectorAll('.analysis-card, .article-title, .article-summary, .article-actions');

    const observer = new IntersectionObserver((entries) => {
        entries.forEach(entry => {
            if (entry.isIntersecting) {
                entry.target.classList.add('fade-in');
                observer.unobserve(entry.target);
            }
        });
    }, { threshold: 0.1 });

    elements.forEach(el => {
        observer.observe(el);
        el.style.opacity = '0';
    });
}

// Animazione di entrata per un cambio tab
function animateEntrance(element) {
    const children = element.querySelectorAll('.analysis-card, .bias-detailed-card, .technique-card, .example-bubble');

    children.forEach((el, index) => {
        el.style.opacity = '0';
        el.style.transform = 'translateY(20px)';

        setTimeout(() => {
            el.style.transition = 'all 0.5s ease';
            el.style.opacity = '1';
            el.style.transform = 'translateY(0)';
        }, 100 + (index * 50));
    });
}

// Aggiungi questa funzione per animare gli elementi nelle tab di framing, retorica e alternative
function initTabAnimations() {
    // Aggiungi gestori per l'espansione delle tecniche retoriche
    document.querySelectorAll('.expand-btn').forEach(btn => {
        btn.addEventListener('click', function () {
            const isExpanded = this.getAttribute('aria-expanded') === 'true';
            this.innerHTML = isExpanded ?
                '<i class="bi bi-chevron-down"></i>' :
                '<i class="bi bi-chevron-up"></i>';
        });
    });

    // Animazione delle bolle nei termini chiave
    document.querySelectorAll('.term-bubble').forEach((bubble, index) => {
        bubble.style.animationDelay = `${index * 0.2}s`;
    });

    // Animazione dello scorrimento nei framing
    document.querySelectorAll('.section-title').forEach(title => {
        const observer = new IntersectionObserver((entries) => {
            entries.forEach(entry => {
                if (entry.isIntersecting) {
                    entry.target.classList.add('fade-in');
                    observer.unobserve(entry.target);
                }
            });
        });

        observer.observe(title);
    });

    // Effetto hover per le carte delle prospettive alternative
    document.querySelectorAll('.perspective-card').forEach(card => {
        card.addEventListener('mouseenter', function () {
            this.style.transform = 'translateY(-5px)';
            this.style.boxShadow = var('--shadow-lg');
        });

        card.addEventListener('mouseleave', function () {
            this.style.transform = '';
            this.style.boxShadow = '';
        });
    });
}

// Aggiungi questa chiamata in document.ready
document.addEventListener('DOMContentLoaded', function () {
    // ... codice esistente ...

    // Inizializza le animazioni per le nuove tab
    initTabAnimations();
});


// Inizializza contatori animati
function initCounters() {
    document.querySelectorAll('.score-value').forEach(el => {
        const target = parseInt(el.textContent, 10);
        let count = 0;
        const duration = 1500; // millisecondi
        const increment = target / (duration / 16);

        const originalText = el.innerHTML;
        const hasSpan = originalText.includes('<span>');
        const suffix = hasSpan ? originalText.substring(originalText.indexOf('<span>')) : '';

        el.textContent = '0';

        const counter = setInterval(() => {
            count += increment;
            if (count >= target) {
                count = target;
                clearInterval(counter);
                if (hasSpan) {
                    el.innerHTML = Math.round(count) + suffix;
                } else {
                    el.textContent = Math.round(count);
                }
            } else {
                if (hasSpan) {
                    el.innerHTML = Math.round(count) + suffix;
                } else {
                    el.textContent = Math.round(count);
                }
            }
        }, 16);
    });
}


// Gestione dei toast di salvataggio
document.addEventListener('DOMContentLoaded', function () {
    // Mostra toast se c'è un messaggio di salvataggio
    const saveToast = document.getElementById('saveToast');
    if (saveToast && saveToast.querySelector('.toast-body').textContent.trim() !== '') {
        const toast = new bootstrap.Toast(saveToast);
        toast.show();
    }
});

// Funzione per mostrare messaggio di login
function showLoginToSaveMessage() {
    const toastContainer = document.createElement('div');
    toastContainer.className = 'position-fixed bottom-0 end-0 p-3';
    toastContainer.style.zIndex = '11';

    const toastElement = document.createElement('div');
    toastElement.className = 'toast show';
    toastElement.innerHTML = `
        <div class="toast-header">
            <i class="bi bi-info-circle me-2"></i>
            <strong class="me-auto">PrismaNews</strong>
            <button type="button" class="btn-close" data-bs-dismiss="toast"></button>
        </div>
        <div class="toast-body">
            Devi accedere per salvare questa analisi.
            <div class="mt-2 pt-2 border-top">
                <a href="/Identity/Account/Login" class="btn btn-primary btn-sm">Accedi</a>
                <a href="/Identity/Account/Register" class="btn btn-secondary btn-sm">Registrati</a>
            </div>
        </div>
    `;

    toastContainer.appendChild(toastElement);
    document.body.appendChild(toastContainer);

    const toast = new bootstrap.Toast(toastElement);
    toast.show();

    setTimeout(() => {
        toastElement.classList.remove('show');
        setTimeout(() => document.body.removeChild(toastContainer), 500);
    }, 5000);
}