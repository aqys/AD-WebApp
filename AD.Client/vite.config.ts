import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'

// https://vite.dev/config/
export default defineConfig({
  plugins: [vue()],
  resolve: {
    alias: {
      '@': fileURLToPath(new URL('./src', import.meta.url))
    }
  },
  server: {
    proxy: {
      '/api': {
        target: 'https://localhost:7123',
        secure: false,       // Ignore self-signed dev cert
        changeOrigin: true,
        configure: (proxy) => {
          // Forward cookies back to the browser unchanged
          proxy.on('proxyRes', (proxyRes) => {
            const setCookie = proxyRes.headers['set-cookie'];
            if (setCookie) {
              // Remove Secure flag so cookies work on http://localhost:5173
              proxyRes.headers['set-cookie'] = setCookie.map((c: string) =>
                c.replace(/;\s*Secure/gi, '').replace(/;\s*SameSite=\w+/gi, '; SameSite=Lax')
              );
            }
          });
        }
      },
      // Also proxy the ADFS signin callback so auth flow completes
      '/signin-adfs': {
        target: 'https://localhost:7123',
        secure: false,
        changeOrigin: true
      }
    }
  },
  build: {
    // Buefy 3.x uses calc() in media queries that lightningcss (Vite 8 default)
    // cannot parse. Disable CSS minification to avoid the build error.
    cssMinify: false
  }
})
