import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    open: true,
    proxy: {
      '/api': {
        // Match WebApi launchSettings HTTPS port to avoid proxy connection refused
        // If you change WebApi port, update this accordingly
        target: 'https://localhost:56157',
        changeOrigin: true,
        secure: false,
        rewrite: path => path.replace(/^\/api/, '')
      }
    }
  }
});
