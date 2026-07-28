import { Component, ElementRef, OnInit, OnDestroy, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { Subscription } from 'rxjs';
import { MENU } from '../../menu';
import { PatientService } from '../../services/patient.service';
import { ToastrService } from 'ngx-toastr';
import { Doctor } from '../../../pages/models';
import { AppointmentService } from '../../../pages/general/services/appointment.service';
import { DoctorService } from '../../../pages/general/services/doctor.service';
import { ReloadService } from '../../../shared/service/reload.service';
import { AuthServiceService } from '../../../pages/auth/auth-services/auth-service.service';
import { DeleteModalComponent } from '../../../doctor/pages/delete-modal/delete-modal.component';
import { TotalEarningsService } from '../../services/total-earnings.service';
import jsPDF from 'jspdf';
import autoTable from 'jspdf-autotable';
import * as XLSX from 'xlsx';
import { SideBarComponent } from '../side-bar/side-bar.component';
import { NgClass, CurrencyPipe, DatePipe } from '@angular/common';
import { ChartComponent } from '../chart/chart.component';
import { ReactiveFormsModule, FormsModule } from '@angular/forms';
import { AssetUrlPipe } from '../../../shared/asset-url.pipe';

@Component({
    selector: 'app-board',
    templateUrl: './board.component.html',
    changeDetection: ChangeDetectionStrategy.Eager,
    styleUrls: ['./board.component.css'],
    imports: [SideBarComponent, NgClass, ChartComponent, DeleteModalComponent, ReactiveFormsModule, FormsModule, CurrencyPipe, DatePipe, AssetUrlPipe]
})
export class BoardComponent implements OnInit, OnDestroy {
  appointments: any[] = [];
  currentPage = 1;
  pageSize = 20;
  pageCount = 0;
  totalCount = 0;
  infoBoxes: any[] = [];
  doctorsData: Doctor[] = [];
  numOfAppointments = 0;
  numOfDoctors = 0;
  numOfPatients = 0;
  totalAmountEarning = 0;
  selectedAppointmentId!: number;
  menuItems = MENU;

  hasAppointments = false;
  hasDoctors = false;
  hasPatients = false;
  hasEarnings = false;
  appointmentsLoadError = false;
  doctorsLoadError = false;
  patientsLoadError = false;
  earningsLoadError = false;
  dataLoaded = false;

  private subscriptions: Subscription[] = [];

  constructor(
    private appointmentService: AppointmentService,
    private doctorService: DoctorService,
    private reload: ReloadService,
    private toaster: ToastrService,
    private patientService: PatientService,
    private totalEarningService: TotalEarningsService,
    private authService: AuthServiceService
  ) {}

  ngAfterViewInit(): void {
    this.reload.initializeLoader();
  }

  ngOnInit(): void {
    const sessionSub = this.authService.resolveSession().subscribe((user) => {
      if (user && this.authService.isRole('admin')) {
        this.loadDashboardData();
      }
    });
    this.subscriptions.push(sessionSub);
  }

  private loadDashboardData(): void {
    this.loadAppointments();
    this.loadDoctor();
    this.fetchPatientLength();
    this.getTotalEarning();
  }

  loadAppointments(page = 1): void {
    this.appointmentsLoadError = false;
    const appointmentSub = this.appointmentService.getAppointments(page, this.pageSize).subscribe({
      next: (data) => {
        this.appointments = data.items;
        this.currentPage = data['currentPage'];
        this.pageCount = data['pageCount'];
        this.totalCount = data['totalCount'];
        this.numOfAppointments = data['totalCount'];
        this.hasAppointments = (data.items?.length ?? 0) > 0;
        this.dataLoaded = true;
        this.optimizeWidget();
        this.setBadgeForAppointments();
      },
      error: () => {
        this.appointmentsLoadError = true;
        this.dataLoaded = true;
      },
    });
    this.subscriptions.push(appointmentSub);
  }

  goToAppointmentPage(page: number): void {
    if (page < 1 || (this.pageCount > 0 && page > this.pageCount)) {
      return;
    }
    this.loadAppointments(page);
  }

  loadDoctor(): void {
    this.doctorsLoadError = false;
    const doctorSub = this.doctorService.getAllDoctors().subscribe({
      next: (result) => {
        this.doctorsData = result?.items ?? [];
        this.numOfDoctors = result['totalCount'] ?? this.doctorsData.length;
        this.hasDoctors = this.doctorsData.length > 0;
        this.dataLoaded = true;
      },
      error: () => {
        this.doctorsLoadError = true;
        this.dataLoaded = true;
      },
    });
    this.subscriptions.push(doctorSub);
  }

  fetchPatientLength(): void {
    this.patientsLoadError = false;
    const patientSub = this.patientService.getAllPatient().subscribe({
      next: (data) => {
        this.numOfPatients = data['totalCount'] ?? data.items?.length ?? 0;
        this.hasPatients = this.numOfPatients > 0;
        this.optimizeWidget();
        this.dataLoaded = true;
      },
      error: () => {
        this.patientsLoadError = true;
        this.dataLoaded = true;
      },
    });
    this.subscriptions.push(patientSub);
  }

  getTotalEarning(): void {
    this.earningsLoadError = false;
    const earningSub = this.totalEarningService.getTotalEarnings().subscribe({
      next: (data) => {
        this.totalAmountEarning = data.totalEarnings ?? 0;
        this.hasEarnings = this.totalAmountEarning > 0;
        this.optimizeWidget();
        this.dataLoaded = true;
      },
      error: () => {
        this.earningsLoadError = true;
        this.dataLoaded = true;
      },
    });
    this.subscriptions.push(earningSub);
  }

  setBadgeForAppointments(): void {
    const appointmentItem = this.menuItems.find(item => item.title === 'Appointment');
    if (appointmentItem) {
      appointmentItem.badge = this.numOfAppointments.toString();
    }
  }

  optimizeWidget(): void {
    this.infoBoxes = [
      { accentClass: 'admin-stat-card--blue', iconClass: 'fa-solid fa-calendar-check', text: 'Appointments', number: this.numOfAppointments },
      { accentClass: 'admin-stat-card--orange', iconClass: 'fa-solid fa-hospital-user', text: 'Patients', number: this.numOfPatients },
      { accentClass: 'admin-stat-card--purple', iconClass: 'fa-solid fa-user-doctor', text: 'Doctors', number: this.numOfDoctors },
      { accentClass: 'admin-stat-card--green', iconClass: 'fa-solid fa-sack-dollar', text: 'Total Earnings', number: this.totalAmountEarning, isCurrency: true },
    ];
  }

  openDeleteModal(id: number): void {
    this.selectedAppointmentId = id;
    this.deleteModal.showModal();
  }

  @ViewChild(DeleteModalComponent) deleteModal!: DeleteModalComponent;

  onDeleteAppointment(id: number): void {
    const deleteSub = this.appointmentService.deleteBookingById(id).subscribe({
      next: () => {
        this.toaster.success('Appointment deleted successfully');
        this.loadAppointments();
      },
      error: () => this.toaster.error('Error deleting appointment'),
    });
    this.subscriptions.push(deleteSub);
  }

  appointmentDate = '';
  appointmentTime = '';
  showEditModal = false;

  onEditeAppointment(id: number, appointmentDate: string): void {
    this.selectedAppointmentId = id;
    this.appointmentDate = appointmentDate.split('T')[0];
    this.appointmentTime = appointmentDate.split('T')[1]?.substring(0, 5) || '';
    this.showEditModal = true;
  }

  closeModal(): void { this.showEditModal = false; }

  saveAppointment(): void {
    const updatedAppointment = {
      id: this.selectedAppointmentId,
      appointmentTakenDate: this.appointmentDate + 'T' + this.appointmentTime,
    };
    if (!this.selectedAppointmentId) {
      this.toaster.error('No appointment selected!');
      return;
    }
    const editSub = this.appointmentService.editeBooking(this.selectedAppointmentId, updatedAppointment).subscribe({
      next: () => {
        this.toaster.success('Appointment Updated successfully');
        this.closeModal();
        this.loadAppointments();
      },
      error: () => this.toaster.error('Error Updating appointment'),
    });
    this.subscriptions.push(editSub);
    this.closeModal();
  }

  downloadAsPDF(): void {
    const doc = new jsPDF();
    autoTable(doc, {
      head: [['Patient Name', 'Assigned Doctor', 'Date', 'Time']],
      body: this.appointments.map(i => [i.patient?.name, i.doctor?.name, i.appointmentDate, i.appointmentTime ?? '']),
    });
    doc.save('table.pdf');
  }

  @ViewChild('tableRef') tableRef!: ElementRef;

  downloadAsExcel(): void {
    const ws: XLSX.WorkSheet = XLSX.utils.table_to_sheet(this.tableRef.nativeElement);
    const wb: XLSX.WorkBook = XLSX.utils.book_new();
    XLSX.utils.book_append_sheet(wb, ws, 'appointments');
    XLSX.writeFile(wb, 'appointments.xlsx');
  }

  ngOnDestroy(): void {
    this.subscriptions.forEach(sub => sub.unsubscribe());
  }
}
