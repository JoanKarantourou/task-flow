/** @type {import('tailwindcss').Config} */
export default {
  // Specify which files Tailwind should scan for class names
  // This allows Tailwind to generate only the CSS you actually use (tree-shaking)
  content: [
    "./index.html",                    // Check the root HTML file
    "./src/**/*.{js,ts,jsx,tsx}",      // Check all JS/TS/React files in src folder
  ],
  
  theme: {
    extend: {
      // Custom colors for TaskFlow brand (we can add more later)
      colors: {
        primary: {
          50: '#eff6ff',
          100: '#dbeafe',
          200: '#bfdbfe',
          300: '#93c5fd',
          400: '#60a5fa',
          500: '#3b82f6',  // Main primary color
          600: '#2563eb',
          700: '#1d4ed8',
          800: '#1e40af',
          900: '#1e3a8a',
        },
      },
      // Custom fonts (optional - we can change this later)
      fontFamily: {
        sans: ['Inter', 'ui-sans-serif', 'system-ui', 'sans-serif'],
      },
    },
  },
  plugins: [],
}