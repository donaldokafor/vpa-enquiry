node {
       stage 'Check out'
    
          echo 'Checking out...'
    
          checkout scm
  
       stage 'Build vpa enquiry gateway'
        
          echo 'Building vpa enquiry gateway...'
   
          dir ('VPA') { 
                 sh '''dotnet restore
                        dotnet build'''
  
              }          
       stage 'test vpa enquiry gateway'
          echo 'Tesing vpa enquiry gateway..'
          dir ('VPA.Test') { 
              sh 'dotnet test VPA.Test.csproj -l "trx;LogFileName=VPATest.trx"'    
          }
       docker.withRegistry("${env.REGISTRY_PROTOCOL}://${env.REGISTRY_HOST}:${env.REGISTRY_PORT}", 'docker_registry_credentials_id') 
       {
        
         stage 'Build Docker Image'
        
           echo 'Building docker image....'
        
           String imageName = "${env.REGISTRY_HOST}:${env.REGISTRY_PORT}/vpa-enquiry-gateway:latest"
  
           dir ('VPA') {      
              sh "docker build -t ${imageName}  ."
  
           }      
           def img = docker.image(imageName)
        
         stage 'Push Docker Image'
        
           echo 'Pushing docker image....'
        
           img.push()
    
       }

       stage 'Deploy to Kubernetes'
        
          echo 'Deploying....'
    
          sh "helm upgrade --install  vpa-enquiry-gateway ./helm --set image.repository=${env.KUBERNETES_REGISTRY_URL}/vpa-enquiry-gateway --set replicaCount=${env.REPLICAS}"
            sh "helm history vpa-enquiry-gateway" 
}