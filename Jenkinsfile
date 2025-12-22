pipeline {
      agent any

      environment {
          SPREADSHEET_ID = credentials('spreadsheet-id')
          GOOGLE_CREDENTIALS = credentials('google-sheets-credentials')
      }

      stages {
          stage('Checkout') {
              steps {
                  git branch: 'data-sync',
                      url: 'https://github.com/dhwodnjs0827/Project_Architecture.git',
                      credentialsId: 'github-credentials'
              }
          }

          stage('Setup Python') {
                steps {
                    sh '''
                        python3 -m venv venv
                        . venv/bin/activate
                        pip install -r parser/requirements.txt
                    '''
                }
          }

          stage('Parse Google Sheets') {
              steps {
                  sh '''
                      . venv/bin/activate
                      export GOOGLE_CREDENTIALS_PATH=$GOOGLE_CREDENTIALS
                      export SPREADSHEET_ID=$SPREADSHEET_ID
                      python3 parser/sheet_parser.py
                  '''
              }
          }

          stage('Commit & Push') {
              steps {
                  withCredentials([usernamePassword(
                      credentialsId: 'github-credentials',
                      usernameVariable: 'GIT_USERNAME',
                      passwordVariable: 'GIT_PASSWORD'
                  )]) {
                      sh '''
                          git config user.name "Jenkins Bot"
                          git config user.email "jenkins@example.com"
                          
                          git add -A
                          
                          if git diff --staged --quiet; then
                              echo "No changes to commit"
                          else
                              git commit -m "chore: sync data from Google Sheets"
                              git push https://${GIT_USERNAME}:${GIT_PASSWORD}@github.com/dhwodnjs0827/Project_Architecture.git data-sync
                          fi
                      '''
                  }
              }
          }
      }

      post {
          success {
              echo '✓ Data sync completed successfully!'
          }
          failure {
              echo '✗ Data sync failed!'
          }
          always {
              cleanWs()
          }
      }
  }
