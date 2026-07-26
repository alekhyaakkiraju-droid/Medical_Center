import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { DeleteModalComponent } from '../doctor/pages/delete-modal/delete-modal.component';
import { PaymentComponent } from '../pages/general/Payment/Payment.component';
import { SideBarComponent } from '../admin/pages/side-bar/side-bar.component';
import { AuthModule } from '../pages/auth/auth.module';
import { AssetUrlPipe } from './asset-url.pipe';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        RouterModule,
        AuthModule,
        DeleteModalComponent,
        PaymentComponent,
        SideBarComponent,
        AssetUrlPipe,
    ],
    exports: [
        DeleteModalComponent,
        PaymentComponent,
        SideBarComponent,
        AssetUrlPipe,
    ],
})
export class SharedModule { }
