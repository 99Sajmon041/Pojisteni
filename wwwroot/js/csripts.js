window.addEventListener('DOMContentLoaded', () => {
    const alert = document.getElementById('success-alert');
    if (alert) {
        setTimeout(() => {
            alert.classList.remove('show');
            setTimeout(() => {
                alert.remove(); 
            }, 1000);
        }, 3000);
    }
});