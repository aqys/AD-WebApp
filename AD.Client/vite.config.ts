import { defineConfig } from 'vite'
import vue from '@vitejs/plugin-vue'
import { fileURLToPath, URL } from 'node:url'


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
        secure: false,       
        changeOrigin: true,
        configure: (proxy) => {
          
          proxy.on('proxyRes', (proxyRes) => {
            const setCookie = proxyRes.headers['set-cookie'];
            if (setCookie) {
              
              proxyRes.headers['set-cookie'] = setCookie.map((c: string) =>
                c.replace(/;\s*Secure/gi, '').replace(/;\s*SameSite=\w+/gi, '; SameSite=Lax')
              );
            }
          });
        }
      },
      
      '/signin-adfs': {
        target: 'https://localhost:7123',
        secure: false,
        changeOrigin: true
      }
    }
  },
  build: {
    
    
    cssMinify: false
  }
})
