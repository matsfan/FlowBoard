import { defineConfig } from 'vite';
import react from '@vitejs/plugin-react';

export default defineConfig({
  plugins: [react()],
  server: {
    port: 5173,
    open: true,
    proxy: {
      '/api': {
        // Match WebApi HTTP port (we run Kestrel without HTTPS in dev)
        // If you change WebApi port, update this accordingly
        target: 'http://localhost:56158',
        changeOrigin: true,
        secure: false,
        rewrite: path => path.replace(/^\/api/, '')
      }
    }
  }
});
