/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    "./src/**/*.{html,ts}", 
    "./node_modules/flowbite/**/*.js" 
  ],
  theme: {
    extend: {
      colors: {
        primary: {
          50: "#eefbfb",
          100: "#d5f3f5",
          200: "#ade7eb",
          300: "#7dd4db",
          400: "#48bdc5",
          500: "#3a9aa1",
          600: "#2f7c82",
          700: "#29656a",
          800: "#255257",
          900: "#224549",
          950: "#0f2a2e",
        },
      }
    },
  },
  plugins: [
    require('flowbite/plugin')
  ]
  
};
