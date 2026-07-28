export const MENU = [
    {
      title: 'Dashboard',
      path: '/admin/dashboard',
      icon: 'fa-solid fa-gauge-high'
    },
    {
      title: 'Appointment',
      path: '/admin/appointments',
      icon: 'fa-solid fa-calendar-check',
      badge: '0'
    },
    {
      title: 'Doctors',
      path: '/admin/doctors',
      icon: 'fa-solid fa-user-doctor'
    },
    {
      title: 'Patients',
      path: '/admin/patients',
      icon: 'fa-solid fa-hospital-user'
    },
    {
      title: 'Sign Out',
      path: '#',
      toggle:'modal',
      target:'#logoutModal',
      icon: 'fa-solid fa-right-from-bracket'
    }
  ];


  export const DoctorMENU = [
    {
      title: 'Dashboard',
      path: '/doctor/dashboard',
      icon: 'fa-solid fa-gauge-high'
    },
    {
      title: 'My Appointments',
      path: '/doctor/doctor-appointments',
      icon: 'fa-solid fa-calendar-check'
    },
    {
      title: 'My Schedule',
      path: '/doctor/doctor-appointments',
      icon: 'fa-solid fa-clock'
    },
    {
      title: 'Profile',
      path: '/doctor/doctor-profile',
      icon: 'fa-solid fa-user'
    },
    {
      title: 'Sign Out',
      path: '#',
      toggle:'modal',
      target:'#logoutModal',
      icon: 'fa-solid fa-right-from-bracket'
    }
  ];
