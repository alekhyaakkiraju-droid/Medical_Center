import { NgModule } from '@angular/core';
import { BrowserModule, provideClientHydration, withNoIncrementalHydration } from '@angular/platform-browser';
import { AppRoutingModule } from './app-routing.module';
import { AppComponent } from './app.component';
import { ErrorPageComponent } from './pages/general/errorPage/errorPage.component';
import { FooterComponent } from './layout/footer/footer.component';
import { HeaderComponent } from './layout/header/header.component';
import { RouterModule } from '@angular/router';
import { GeneralModule } from './pages/general/general.module';
import { AuthModule } from './pages/auth/auth.module';
import { FormsModule } from '@angular/forms';
import { HttpClientModule, provideHttpClient, withFetch, withInterceptors } from '@angular/common/http';
import { credentialsInterceptor } from './core/interceptors/credentials.interceptor';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import { provideToastr, ToastrModule } from 'ngx-toastr';

@NgModule({
    imports: [
        BrowserModule,
        AppRoutingModule,
        RouterModule,
        AuthModule,
        GeneralModule,
        BrowserModule,
        FormsModule,
        HttpClientModule,
        BrowserAnimationsModule,
        ToastrModule.forRoot({
            positionClass: 'toast-bottom-left',
            preventDuplicates: true,
            closeButton: true,
            timeOut: 5000,
            progressBar: true
        }),
        AppComponent,
        FooterComponent,
        HeaderComponent,
        ErrorPageComponent,
    ],
    providers: [
        provideClientHydration(withNoIncrementalHydration()),
        provideAnimationsAsync(),
        provideToastr(),
        provideHttpClient(withFetch(), withInterceptors([credentialsInterceptor])),
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }


