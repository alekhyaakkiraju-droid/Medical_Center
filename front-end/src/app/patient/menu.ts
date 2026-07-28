export const PATIENT_MENU = [
  { title: 'Home', path: '/patient/home', icon: 'fa-solid fa-house' },
  { title: 'My Appointments', path: '/patient/appointments', icon: 'fa-solid fa-calendar-check' },
  { title: 'Book Appointment', path: '/pages/appointment', icon: 'fa-solid fa-plus' },
  { title: 'Profile', path: '/pages/user-profile', icon: 'fa-solid fa-user' },
  {
    title: 'Sign Out',
    path: '#',
    toggle: 'modal',
    target: '#logoutModal',
    icon: 'fa-solid fa-right-from-bracket',
  },
];
