document.addEventListener('DOMContentLoaded', function() {
    // Toggle password visibility
    const togglePasswordButtons = document.querySelectorAll('.toggle-password');
    togglePasswordButtons.forEach(button => {
        button.addEventListener('click', function() {
            const input = this.parentElement.querySelector('input');
            const icon = this.querySelector('i');
            
            if (input.type === 'password') {
                input.type = 'text';
                icon.classList.remove('bi-eye');
                icon.classList.add('bi-eye-slash');
            } else {
                input.type = 'password';
                icon.classList.remove('bi-eye-slash');
                icon.classList.add('bi-eye');
            }
        });
    });

    // Password strength meter
    const passwordInput = document.getElementById('Input_Password');
    if (passwordInput) {
        const strengthMeter = document.getElementById('strengthMeter');
        const feedback = document.getElementById('passwordFeedback');
        
        passwordInput.addEventListener('input', function() {
            const value = passwordInput.value;
            const result = calculatePasswordStrength(value);
            
            // Clear previous classes
            strengthMeter.className = 'strength-meter';
            
            if (value.length === 0) {
                feedback.textContent = '';
            } else if (result.score === 1) {
                strengthMeter.classList.add('strength-weak');
                feedback.textContent = 'Password debole: ' + result.feedback;
                feedback.style.color = '#FF5252';
            } else if (result.score === 2) {
                strengthMeter.classList.add('strength-medium');
                feedback.textContent = 'Password media: ' + result.feedback;
                feedback.style.color = '#FFC107';
            } else {
                strengthMeter.classList.add('strength-strong');
                feedback.textContent = 'Password forte!';
                feedback.style.color = '#4CAF50';
            }
        });
    }

    // Terms modal accept
    window.acceptTerms = function() {
        const termsCheckbox = document.getElementById('Input_AcceptTerms');
        if (termsCheckbox) {
            termsCheckbox.checked = true;
            // Trigger validation
            termsCheckbox.dispatchEvent(new Event('change'));
        }
    };

    // Add floating animation to login/register forms
    const authCard = document.querySelector('.auth-card');
    if (authCard) {
        window.addEventListener('mousemove', function(e) {
            const x = e.clientX / window.innerWidth;
            const y = e.clientY / window.innerHeight;
            
            // Calculate tilt angles (max 3 degrees)
            const tiltX = (y - 0.5) * 3;
            const tiltY = (x - 0.5) * -3;
            
            // Apply the tilt effect with smooth transitions
            authCard.style.transform = `perspective(1000px) rotateX(${tiltX}deg) rotateY(${tiltY}deg) translateY(-5px)`;
        });
        
        // Reset tilt when mouse leaves
        window.addEventListener('mouseleave', function() {
            authCard.style.transform = 'perspective(1000px) rotateX(0deg) rotateY(0deg) translateY(-5px)';
        });
    }

    // Function to calculate password strength
    function calculatePasswordStrength(password) {
        let score = 0;
        let feedback = '';
        
        // Length check
        if (password.length < 6) {
            feedback = 'troppo corta.';
            return { score: 1, feedback };
        } else if (password.length >= 10) {
            score += 1;
        }
        
        // Complexity checks
        if (/[A-Z]/.test(password)) score += 1;
        if (/[a-z]/.test(password)) score += 1;
        if (/[0-9]/.test(password)) score += 1;
        if (/[^A-Za-z0-9]/.test(password)) score += 1;
        
        // Generate feedback
        if (score < 3) {
            feedback = 'aggiungi maiuscole, numeri e simboli.';
            return { score: 1, feedback };
        } else if (score < 4) {
            feedback = 'quasi buona, aggiungi altri tipi di caratteri.';
            return { score: 2, feedback };
        } else {
            return { score: 3, feedback: '' };
        }
    }

    // Add floating label behavior for empty inputs (fix for autocomplete)
    const inputs = document.querySelectorAll('.floating-input input');
    inputs.forEach(input => {
        if (input.value !== '') {
            input.parentNode.classList.add('filled');
        }
        input.addEventListener('blur', function() {
            if (this.value !== '') {
                this.parentNode.classList.add('filled');
            } else {
                this.parentNode.classList.remove('filled');
            }
        });
    });

    // Add input validation styles
    const forms = document.querySelectorAll('form');
    forms.forEach(form => {
        const validateInput = (input) => {
            if (!input.validity.valid) {
                input.classList.add('is-invalid');
            } else {
                input.classList.remove('is-invalid');
                input.classList.add('is-valid');
            }
        };

        // Add validation for all inputs
        form.querySelectorAll('input').forEach(input => {
            input.addEventListener('blur', () => validateInput(input));
        });
    });
});
