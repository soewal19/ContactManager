(() => {
    const colorStops = [
        [58, 130, 246],
        [255, 148, 180],
        [0, 255, 136],
        [255, 255, 0]
    ];

    const createParticle = (x, y, strength) => {
        const angle = Math.random() * Math.PI * 2;
        const speed = (0.6 + Math.random() * 1.4) * strength;
        const radius = 6 + Math.random() * 14 * strength;
        const life = 0.9 + Math.random() * 0.4;
        const color = colorStops[Math.floor(Math.random() * colorStops.length)];
        return {
            x,
            y,
            vx: Math.cos(angle) * speed,
            vy: Math.sin(angle) * speed,
            radius,
            life,
            decay: 0.015 + Math.random() * 0.02,
            color
        };
    };

    const initSplashCursor = (host) => {
        const canvas = host.querySelector('.splash-cursor-canvas') || document.createElement('canvas');
        if (!canvas.parentElement) {
            canvas.className = 'splash-cursor-canvas';
            canvas.setAttribute('aria-hidden', 'true');
            host.appendChild(canvas);
        }

        const ctx = canvas.getContext('2d');
        let particles = [];
        let dpr = Math.max(window.devicePixelRatio || 1, 1);
        let width = 0;
        let height = 0;
        let lastSpawn = 0;

        const resize = () => {
            const rect = host.getBoundingClientRect();
            width = rect.width;
            height = rect.height;
            dpr = Math.max(window.devicePixelRatio || 1, 1);
            canvas.width = width * dpr;
            canvas.height = height * dpr;
            canvas.style.width = `${width}px`;
            canvas.style.height = `${height}px`;
            ctx.setTransform(dpr, 0, 0, dpr, 0, 0);
        };

        const observer = new ResizeObserver(resize);
        observer.observe(host);
        resize();

        const spawn = (x, y, intensity) => {
            const count = Math.round(8 + intensity * 12);
            for (let i = 0; i < count; i += 1) {
                particles.push(createParticle(x, y, intensity));
            }
            if (particles.length > 220) {
                particles = particles.slice(particles.length - 220);
            }
        };

        const render = () => {
            ctx.clearRect(0, 0, width, height);
            ctx.globalCompositeOperation = 'lighter';
            for (let i = particles.length - 1; i >= 0; i -= 1) {
                const p = particles[i];
                p.x += p.vx;
                p.y += p.vy;
                p.vx *= 0.96;
                p.vy *= 0.96;
                p.life -= p.decay;
                if (p.life <= 0) {
                    particles.splice(i, 1);
                    continue;
                }
                const gradient = ctx.createRadialGradient(p.x, p.y, 0, p.x, p.y, p.radius);
                const [r, g, b] = p.color;
                gradient.addColorStop(0, `rgba(${r}, ${g}, ${b}, ${0.45 * p.life})`);
                gradient.addColorStop(0.6, `rgba(${r}, ${g}, ${b}, ${0.15 * p.life})`);
                gradient.addColorStop(1, 'rgba(0, 0, 0, 0)');
                ctx.fillStyle = gradient;
                ctx.beginPath();
                ctx.arc(p.x, p.y, p.radius, 0, Math.PI * 2);
                ctx.fill();
            }
            requestAnimationFrame(render);
        };

        render();

        const handleMove = (event) => {
            const now = performance.now();
            if (now - lastSpawn < 16) {
                return;
            }
            lastSpawn = now;
            const rect = host.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            if (x < 0 || y < 0 || x > rect.width || y > rect.height) {
                return;
            }
            spawn(x, y, 0.9);
        };

        const handleDown = (event) => {
            const rect = host.getBoundingClientRect();
            const x = event.clientX - rect.left;
            const y = event.clientY - rect.top;
            spawn(x, y, 1.4);
        };

        host.addEventListener('pointermove', handleMove);
        host.addEventListener('pointerdown', handleDown);
    };

    const initializeSplashCursors = () => {
        const hosts = document.querySelectorAll('[data-splash-cursor]');
        hosts.forEach((host) => initSplashCursor(host));
    };

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initializeSplashCursors);
    } else {
        initializeSplashCursors();
    }
})();
