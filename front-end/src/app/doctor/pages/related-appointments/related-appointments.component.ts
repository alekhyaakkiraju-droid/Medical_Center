import { Component, OnDestroy, OnInit, ViewChild } from '@angular/core';
import { ReloadService } from '../../../shared/service/reload.service';
import { DoctorAppointmentsService } from '../../services/doctor-appointments.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { SearchService } from '../../services/search.service';
import { ToastrService } from 'ngx-toastr';
import { DeleteModalComponent } from '../delete-modal/delete-modal.component';
import { Subscription } from 'rxjs';
import { Booking } from '../../../pages/models';


@Component({
  selector: 'app-related-appointments',
  templateUrl: './related-appointments.component.html'
})

export class RelatedAppointmentsComponent implements OnInit, OnDestroy {

  @ViewChild(DeleteModalComponent) deleteModal!: DeleteModalComponent;

  private subscriptions: Subscription[] = [];
  
  constructor(
    private reload: ReloadService,
    private doctorService: DoctorAppointmentsService,
    private authService: AuthServiceService,
    private toaster: ToastrService,
    private searchService: SearchService
  ) { }

  doctorId: string = '';
  allBookings: Booking[] = [];
  displayBookings: Booking[] = [];
  errorMessage: string = '';
  selectedAppointmentId!: number;
  selectedFilter: string = '1';
  isDropdownOpen = false;
  currentPage = 1;
  pageSize = 10;
  totalCount = 0;
  pageCount = 0;

  filters = [
    { id: '1', label: 'All days' },
    { id: '2', label: 'Today' },
    { id: '3', label: 'Up Coming' },
    { id: '4', label: 'Last 30 days' }
  ];

  ngAfterViewInit(): void {
    this.reload.initializeLoader();
  }

  ngOnInit(): void {
    this.setDoctorId();
    this.loadBookings();
  }

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }

  setDoctorId(): void {
    const id = this.authService.getNameIdentifier();
    if (id) {
      this.doctorId = id;
    } else {
      console.error(this.errorMessage);
    }
  }

  loadBookings(): void {
    const sub = this.doctorService.getAllDoctorBookings(this.doctorId, 1, 100).subscribe({
      next: (response) => {
        this.allBookings = response.items;
        this.applyFiltersAndPagination();
      },
      error: (error) => {
        console.error(error);
      }
    });
    this.subscriptions.push(sub);
  }

  deleteAppointment(appointmentId: number): void {
    const sub = this.doctorService.deleteBooking(this.doctorId, appointmentId).subscribe(
      () => {
        this.toaster.success("Appointment deleted successfully");
        this.loadBookings();
      },
      (error) => {
        console.error('Error deleting appointment', error);
        this.toaster.error("Error deleting appointment");
      }
    );
    this.subscriptions.push(sub);
  }

  openDeleteModal(id: number) {
    this.selectedAppointmentId = id;
    this.deleteModal.showModal();
  }

  onFilterChange(selected: string): void {
    this.selectedFilter = selected;
    this.currentPage = 1;
    this.closeDropdown();
    this.applyFiltersAndPagination();
  }

  getSelectedLabel(): string {
    const selectedFilterObject = this.filters.find(filter => filter.id === this.selectedFilter);
    return selectedFilterObject ? selectedFilterObject.label : 'Select Filter';
  }

  applyFiltersAndPagination(): void {
    let filtered = [...this.allBookings];
    const today = new Date();
    today.setHours(0, 0, 0, 0);

    switch (this.selectedFilter) {
      case '2':
        filtered = filtered.filter(booking => this.isSameDay(booking.appointmentTakenDate, today));
        break;
      case '3':
        filtered = filtered.filter(booking => {
          const date = booking.appointmentTakenDate ? new Date(booking.appointmentTakenDate) : null;
          return date ? date >= today : false;
        });
        break;
      case '4': {
        const thirtyDaysAgo = new Date(today);
        thirtyDaysAgo.setDate(today.getDate() - 30);
        filtered = filtered.filter(booking => {
          const date = booking.appointmentTakenDate ? new Date(booking.appointmentTakenDate) : null;
          return date ? date >= thirtyDaysAgo && date <= today : false;
        });
        break;
      }
    }

    if (this.searchItem?.trim()) {
      const query = this.searchItem.toLowerCase().trim();
      filtered = filtered.filter(booking =>
        booking.patientName?.toLowerCase().includes(query)
      );
    }

    this.totalCount = filtered.length;
    this.pageCount = this.totalCount === 0 ? 0 : Math.ceil(this.totalCount / this.pageSize);
    const start = (this.currentPage - 1) * this.pageSize;
    this.displayBookings = filtered.slice(start, start + this.pageSize);
  }

  searchItem!: string;
  search(event: Event) {
    const query = this.searchItem.toLowerCase().trim();
    this.searchService.setSearchTerm(query);
    this.currentPage = 1;
    this.applyFiltersAndPagination();
  }

  goToPage(page: number): void {
    if (page < 1 || (this.pageCount > 0 && page > this.pageCount)) {
      return;
    }
    this.currentPage = page;
    this.applyFiltersAndPagination();
  }

  private isSameDay(value: string | null | undefined, comparison: Date): boolean {
    if (!value) {
      return false;
    }
    const date = new Date(value);
    return date.getFullYear() === comparison.getFullYear()
      && date.getMonth() === comparison.getMonth()
      && date.getDate() === comparison.getDate();
  }
}
