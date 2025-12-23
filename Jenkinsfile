// Google Sheets 데이터를 파싱하여 Git에 동기화 하는 파이프라인
pipeline {
      agent any

      // Jenkins Credentials 자격 증명 로드
      environment {
          SPREADSHEET_ID = credentials('') // Jnekins Google Spreadsheet ID (ex: spreadsheet-id)
          GOOGLE_CREDENTIALS = credentials('') // Jenkins Google Sheets API ID (ex: google-sheets-credentials)
      }

      // 1단계: Git에서 {branch} 이름의 브랜치 체크아웃
      stages {
          stage('Checkout') {
              steps {
                  git branch: '', // 체크아웃할 브랜치 이름 (ex: data-sync)
                      url: '', // Git URL (ex: https://github.com/깃 허브 아이디/리포지토리 이름.git)
                      credentialsId: '' // Jenkins GitHub Token ID (ex: github-credentials)
                      
                      // dev 브랜치 merge로 최신화 (충돌 시, 파이프라인 실패 및 중단)
                      withCredentials([usernamePassword(
                                   credentialsId: 'github-credentials',
                                   usernameVariable: 'GIT_USERNAME',
                                   passwordVariable: 'GIT_PASSWORD'
                               )]) {
                                   sh '''
                                       git fetch origin dev
                                       git merge origin/dev --no-edit
                                   '''
                               }
              }
          }

          // 2단계: Pyton 가상환경 설정 및 의존성 설치
          // 프로젝트 경로에 parser/requirements.txt 파일 있는지 확인!
          stage('Setup Python') {
                steps {
                    sh '''
                        python3 -m venv venv
                        . venv/bin/activate
                        pip install -r parser/requirements.txt
                    '''
                }
          }

          // 3단계: Google Sheets에서 데이터를 파싱하여 로컬 파일로 저장
          // 프로젝트 경로에 parser/sheet_parser.py 파일 있는지 확인!
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

          // 4단계: 변경사항이 있으면 커밋하고 푸쉬
          // Git 주소랑 브랜치 변경 필요!
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
                          
                          # 스테이징된 변경사항이 없으면 커밋 스킵
                          if git diff --staged --quiet; then
                              echo "No changes to commit"
                          else
                              git commit -m "chore: sync data from Google Sheets"
                              git push https://${GIT_USERNAME}:${GIT_PASSWORD}@github.com/깃 허브 아이디/리포지토리 이름.git 브랜치 이름
                          fi
                      '''
                  }
              }
          }
      }

      // 파이프라인 완료 후 실행되는 작업들
      post {
          success {
              echo '✓ Data sync completed successfully!'
          }
          failure {
              echo '✗ Data sync failed!'
          }
          always {
              cleanWs() // 워크스페이스 정리
          }
      }
  }
