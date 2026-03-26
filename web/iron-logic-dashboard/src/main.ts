import { bootstrapApplication } from '@angular/platform-browser';
import { appConfig } from './app/app.config';
import { App } from './app/app'; 

bootstrapApplication(App, appConfig)
  .then(() => console.log('IronLogic Dashboard Bootstrapped! 🚀')) 
  .catch((err) => console.error('Bootstrap Error:', err));
