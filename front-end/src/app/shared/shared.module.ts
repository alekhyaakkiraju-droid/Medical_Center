import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DeleteModalComponent } from '../doctor/pages/delete-modal/delete-modal.component';
import { PaymentComponent } from '../pages/general/Payment/Payment.component';
import { SideBarComponent } from '../admin/pages/side-bar/side-bar.component';
import { AuthModule } from '../pages/auth/auth.module';

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterModule,
    AuthModule,
  ],
  declarations: [
    DeleteModalComponent,
    PaymentComponent,
    SideBarComponent,
  ],
  exports: [
    DeleteModalComponent,
    PaymentComponent,
    SideBarComponent,
  ],
})
export class SharedModule { }
