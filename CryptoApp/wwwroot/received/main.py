


broj_prve_grupe = 0
odigrani_potezi = set()
zabranjeni_potezi = set()
trouglici = set()
igraci = ["Igrač 1", "Igrač 2"]
trenutni_igrac = None
trouglici1 = set()
trouglici2 = set()
potezi = set()

def izracunaj_moguce_poteze(graf):
  smerovi = ['D', 'DD', 'DL']


  pomeraj = {
    "DD": (1, 1),
    "DL": (1, 0),
    "D": (0, 1),
  }

  sredina = (broj_prve_grupe*2-1) // 2

  for cvor in graf:
    for smer in smerovi:

      trenutna_pozicija = graf[cvor][0]
      delta_red, delta_kolona = pomeraj[smer]

      putanja = [cvor]
      for _ in range(3):

        if trenutna_pozicija[0] >= sredina and smer == "DD":
          nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1])

        elif trenutna_pozicija[0] >= sredina and smer == "DL":
          nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1] - 1)
        else:
          nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1] + delta_kolona)


        sledeci_cvor = None
        for naziv, vrednost in graf.items():
          if vrednost[0] == nova_pozicija:
            sledeci_cvor = naziv
            break

        if sledeci_cvor is None:
          continue
        putanja.append(sledeci_cvor)
        trenutna_pozicija = nova_pozicija


      if len(putanja)==4:
        potezi.add((cvor, smer))


def dodaj_zabranjen(odigrani_potezi, pocetni_cvor, smer):
  return



def odredi_orijentaciju(trojka, graf):
  usamljeni = pronadji_razlicit(trojka)
  cvor1, cvor2, cvor3 = trojka
  if usamljeni != cvor1:
    if graf[usamljeni][0][0]<graf[cvor1][0][0]:
      return 1
  if usamljeni != cvor2:
    if graf[usamljeni][0][0]<graf[cvor2][0][0]:
      return 1
  if usamljeni != cvor3:
    if graf[usamljeni][0][0]<graf[cvor3][0][0]:
      return 1
  else:
    return 0

def pronadji_razlicit(trojka):

  slova = [element[0] for element in trojka]


  if slova[0] == slova[1]:
    return trojka[2]
  elif slova[0] == slova[2]:
    return trojka[1]
  else:
    return trojka[0]

def proveri_trougao(graf, novi_potezi, igrac, trouglici, trouglici1, trouglici2):

  postoji = False

  for novi_potez in novi_potezi:
    cvor1, cvor2 = novi_potez


    susedi_cvor1 = graf[cvor1][1]
    susedi_cvor2 = graf[cvor2][1]


    zajednicki_susedi = set(susedi_cvor1) & set(susedi_cvor2)

    for cvor in zajednicki_susedi:
      if cvor != cvor1 and cvor != cvor2:
          trougao = tuple(sorted([cvor1, cvor2, cvor], key=lambda x: (x[0], int(x[1:]))))
          trouglici.add(trougao)
          if igrac=="Igrač 1":
            if trougao not in trouglici2:
              trouglici1.add(trougao)
          else:
            if trougao not in trouglici1:
              trouglici2.add(trougao)
          postoji = True

  return postoji



def dodaj_potez_duzine_4(graf, pocetni_cvor, smer, odigrani_potezi, potezi, trouglici, trouglici1, trouglici2, trenutni_igrac):
  potez_key = (pocetni_cvor, smer)
  if potez_key in odigrani_potezi:
    print(f"Potez '{pocetni_cvor} -> {smer}' je već odigran. Ne može se ponoviti.")
    return False

  if potez_key in zabranjeni_potezi:
    print(f"Potez '{pocetni_cvor} -> {smer}' je zabranjen jer se u potpunosti preklapaju sve gumice.")
    return False


  pomeraj = {
    "DD": (1, 1),
    "DL": (1, 0),
    "D": (0, 1),
  }

  sredina = (broj_prve_grupe*2-1) // 2

  if smer not in pomeraj:
    print("Nevažeći smer. Dozvoljeni su 'DD', 'DL' ili 'D'.")
    return

  if pocetni_cvor not in graf:
    print(f"Čvor '{pocetni_cvor}' ne postoji u grafu.")
    return False

  trenutna_pozicija = graf[pocetni_cvor][0]
  delta_red, delta_kolona = pomeraj[smer]


  putanja = [pocetni_cvor]
  for _ in range(3):
    if trenutna_pozicija[0] >= sredina and smer == "DD":
      nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1])

    elif trenutna_pozicija[0] >= sredina and smer == "DL":
      nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1] - 1)
    else:
      nova_pozicija = (trenutna_pozicija[0] + delta_red, trenutna_pozicija[1] + delta_kolona)

    sledeci_cvor = None
    for naziv, vrednost in graf.items():
      if vrednost[0] == nova_pozicija:
        sledeci_cvor = naziv
        break

    if sledeci_cvor is None:
      print(f"Ne može se dodati potez '{pocetni_cvor}' -> '{smer}': izlazak iz opsega.")
      return False

    putanja.append(sledeci_cvor)
    trenutna_pozicija = nova_pozicija

  odigrani_potezi.add(potez_key)

  potezi.remove((pocetni_cvor, smer))

  dodati_potezi = [(putanja[i], putanja[i + 1]) for i in range(len(putanja) - 1)]
  proveri_trougao(graf, dodati_potezi, igraci[trenutni_igrac], trouglici, trouglici1, trouglici2)


  for i in range(len(putanja) - 1):
    if putanja[i + 1] not in graf[putanja[i]][1]:
      graf[putanja[i]][1].append(putanja[i + 1])
    if putanja[i] not in graf[putanja[i + 1]][1]:
      graf[putanja[i + 1]][1].append(putanja[i])

  return True



def formiraj_graf(broj_prve_grupe):
  if broj_prve_grupe < 4 or broj_prve_grupe > 8:
    print("Broj čvorova prve grupe mora biti između 4 i 8.")
    return

  graf_tabela = {}
  broj_grupa = 2 * broj_prve_grupe - 1
  srednja_grupa = broj_grupa // 2
  trenutni_broj_cvorova = broj_prve_grupe

  for grupa in range(broj_grupa):
    grupa_oznaka = chr(65 + grupa)
    for cvor in range(1, trenutni_broj_cvorova + 1):
      pozicija = (grupa, cvor-1)
      graf_tabela[f"{grupa_oznaka}{cvor}"] = (pozicija, [])

    if grupa < srednja_grupa:
      trenutni_broj_cvorova += 1
    else:
      trenutni_broj_cvorova -= 1



  return graf_tabela

def generisi_uvlacenja(broj_prve_grupe):

    broj_elemenata = broj_prve_grupe * 2 - 1
    sredina = broj_elemenata // 2
    uvlacenja = []

    for i in range(broj_elemenata):
      if i < sredina:
        uvlacenje = 3 + 3 * (sredina - i)
      elif i > sredina:
        uvlacenje = 3 + 3 * (i - sredina)
      else:
        uvlacenje = 3
      uvlacenja.append(uvlacenje)

    return uvlacenja


def prikazi_graf_tufne(graf):
  max_red = max(pozicija[0] for pozicija, _ in graf.values())
  max_kolona = max(pozicija[1] for pozicija, _ in graf.values())
  min_red = min(pozicija[0] for pozicija, _ in graf.values())
  min_kolona = min(pozicija[1] for pozicija, _ in graf.values())


  broj_redova = (max_red - min_red + 1)
  broj_kolona = (max_kolona - min_kolona + 1)

  uvlačenja = generisi_uvlacenja(broj_prve_grupe)

  matrica = [[' ' for _ in range(broj_kolona * 6 + uvlačenja[0])] for _ in
             range(broj_redova * 3)]


  prva_linija = [' ' for _ in range(broj_kolona * 6 + uvlačenja[0])]
  druga_linija = [' ' for _ in range(broj_kolona * 6 + uvlačenja[0])]
  treca_linija = [' ' for _ in range(broj_kolona * 6 + uvlačenja[0])]


  for i in range(broj_kolona):
    prva_linija[i * 6 + uvlačenja[0] + 3] = str(i + 1)
  matrica.insert(0, prva_linija)
  matrica.insert(1, druga_linija)
  matrica.insert(2, treca_linija)


  poslednja_linija = [' ' for _ in range(broj_kolona * 6 + uvlačenja[0])]

  matrica.append(poslednja_linija)

  for i in range(broj_kolona):
    poslednja_linija[i * 6 + uvlačenja[0] + 3] = str(i + 1)


  for naziv, (pozicija, susedi) in graf.items():
    red, kolona = pozicija  # Red i kolona čvora
    red -= min_red  # Pomak na početak matrice
    kolona -= min_kolona  # Pomak na početak matrice

    # Dodajemo uvlačenje za red
    uvlačenje = uvlačenja[red]
    trenutna_kolona = kolona * 6 + uvlačenje
    matrica[red * 3 + 3][trenutna_kolona] = '•'  # Postavljamo čvor u matricu

    # Povezujemo susedne čvorove linijama
    for sused in susedi:
      sused_red, sused_kolona = graf[sused][0]
      sused_red -= min_red
      sused_kolona -= min_kolona

      # Uzimamo uvlačenja za trenutni red i susedni red
      uvlačenje_sused = uvlačenja[sused_red]
      susednja_kolona = sused_kolona * 6 + uvlačenje_sused

      # Horizontalna veza desno
      if sused_red == red and sused_kolona > kolona:
        for i in range(1, 6):  # Dodajemo 5 crtica, između dva čvora je 6 praznih mesta
          matrica[red * 3+3][trenutna_kolona + i] = '-'


      elif red >= (2 * broj_prve_grupe - 1) // 2 and sused_red > red and sused_kolona < kolona:
        razlika_redova = (sused_red - red) * 3  # Koristimo 3 reda za vertikalni razmak
        razlika_kolona = trenutna_kolona - susednja_kolona
        for i in range(1, razlika_redova + 1):
          matrica[red * 3 + 3 + i][trenutna_kolona - (i * razlika_kolona // razlika_redova)] = '/'
      elif red < (2 * broj_prve_grupe - 1) // 2 and sused_red > red and sused_kolona == kolona:
        razlika_redova = (sused_red - red) * 3  # Koristimo 3 reda za vertikalni razmak
        razlika_kolona = trenutna_kolona - susednja_kolona
        for i in range(1, razlika_redova + 1):
          matrica[red * 3 + 3 + i][trenutna_kolona - (i * razlika_kolona // razlika_redova)] = '/'


      elif red < (2 * broj_prve_grupe - 1) // 2 and sused_red > red and sused_kolona > kolona:
        razlika_redova = (sused_red - red) * 3
        razlika_kolona = susednja_kolona - trenutna_kolona
        for i in range(1, razlika_redova + 1):
          matrica[red * 3 + 3 + i][trenutna_kolona + (i * razlika_kolona // razlika_redova)] = '\\'
      elif red >= (2 * broj_prve_grupe - 1) // 2 and sused_red > red and sused_kolona == kolona:
        razlika_redova = (sused_red - red) * 3
        razlika_kolona = susednja_kolona - trenutna_kolona
        for i in range(1, razlika_redova + 1):
          matrica[red * 3 + 3 + i][trenutna_kolona + (i * razlika_kolona // razlika_redova)] = '\\'


  for trougao in trouglici:

    cvor1, cvor2, cvor3 = trougao
    red1, kolona1 = graf[cvor1][0]
    red2, kolona2 = graf[cvor2][0]
    red3, kolona3 = graf[cvor3][0]

    usamljeni_cvor = pronadji_razlicit(trougao)

    centar_red = (red1 + red2 + red3) // 3


    pomak_red = centar_red * 3 + 1 + 3
    if odredi_orijentaciju(trougao, graf)==1:
      pomak_red = centar_red * 3 + 2 + 3
    pomak_kolona = graf[usamljeni_cvor][0][1] * 6 + uvlačenja[graf[usamljeni_cvor][0][0]]


    if trougao in trouglici1:
      matrica[pomak_red][pomak_kolona] = 'X'
    else:
      matrica[pomak_red][pomak_kolona] = 'O'


  slova = [chr(65 + i) for i in range(broj_redova)]


  for i, red in enumerate(matrica):
    if i != 0 and (i - 3) % 3 == 0 and (i - 3) // 3 < len(slova):
      red.insert(0, f"{slova[(i - 3) // 3]} ")
    else:
      red.insert(0, "  ")

  for red in matrica:
    print(''.join(red))

  print(f"\n\nBroj trouglova za igraca 1 je {len(trouglici1)}, a za igraca 2 je {len(trouglici2)}")

def zavrsi_igru():
  dodatak = 0

  if broj_prve_grupe == 4:
    dodatak=3
  elif broj_prve_grupe == 5:
    dodatak = 8
  elif broj_prve_grupe == 6:
    dodatak = 15
  elif broj_prve_grupe == 7:
    dodatak = 24
  elif broj_prve_grupe == 8:
    dodatak = 35

  ukupan_broj = (2*(broj_prve_grupe-1)*broj_prve_grupe + dodatak) * 2
  return len(trouglici) >= ukupan_broj or len(trouglici1) > ukupan_broj / 2 or len(trouglici2) > ukupan_broj / 2



def simuliraj_graf(graf):
  graf_kopija = {k: (v[0], v[1][:]) for k, v in graf.items()}
  return graf_kopija


def oceni(trouglovi1, trouglovi2):
    return len(trouglovi2)-len(trouglovi1)

def max_value(graf, trouglovi, trouglovi1, trouglovi2, moguci_potezi, odigrani, dubina, alpha, beta, trenutni_igrac, potez=None):
  if zavrsi_igru():
    return (potez, oceni(trouglovi1, trouglovi2))

  if dubina == 0 or moguci_potezi is None or len(moguci_potezi) == 0:
     return (potez, oceni(trouglovi1, trouglovi2))
  else:
    for s in moguci_potezi:
      pocetni, smer = s
      simulirani = simuliraj_graf(graf)

      lista_poteza = moguci_potezi.copy()
      lista_odigranih = odigrani.copy()
      trouglovi_kopija = trouglovi.copy()
      trouglovi1_kopija = trouglovi1.copy()
      trouglovi2_kopija = trouglovi2.copy()
      trenutni_igrac_kopija = trenutni_igrac

      dodaj_potez_duzine_4(simulirani, pocetni, smer, lista_odigranih, lista_poteza, trouglovi_kopija, trouglovi1_kopija, trouglovi2_kopija, trenutni_igrac_kopija)
      alpha = max(alpha, min_value(simulirani, trouglovi_kopija, trouglovi1_kopija, trouglovi2_kopija, lista_poteza, lista_odigranih, dubina - 1,
                                 alpha, beta, (trenutni_igrac_kopija + 1) % len(igraci), s if potez is None else potez), key=lambda x: x[1])
      if alpha[1] >= beta[1]:
         return beta
  return alpha


def min_value(graf, trouglovi, trouglovi1, trouglovi2, moguci_potezi, odigrani, dubina, alpha, beta, trenutni_igrac, potez=None):
  if zavrsi_igru():
    return (potez, oceni(trouglovi1, trouglovi2))

  if dubina == 0 or moguci_potezi is None or len(moguci_potezi) == 0:
    return (potez, oceni(trouglovi1, trouglovi2))
  else:
    for s in moguci_potezi:
      pocetni, smer = s
      simulirani = simuliraj_graf(graf)
      lista_poteza = moguci_potezi.copy()
      lista_odigranih = odigrani.copy()
      trouglovi_kopija = trouglovi.copy()
      trouglovi1_kopija = trouglovi1.copy()
      trouglovi2_kopija = trouglovi2.copy()
      trenutni_igrac_kopija = trenutni_igrac

      dodaj_potez_duzine_4(simulirani, pocetni, smer, lista_odigranih, lista_poteza, trouglovi_kopija, trouglovi1_kopija, trouglovi2_kopija, trenutni_igrac_kopija)
      beta = min(beta, max_value(simulirani, trouglovi_kopija, trouglovi1_kopija, trouglovi2_kopija, lista_poteza, lista_odigranih, dubina - 1,
                               alpha, beta, (trenutni_igrac_kopija + 1) % len(igraci), s if potez is None else potez), key=lambda x: x[1])
    if beta[1] <= alpha[1]:
      return alpha
  return beta


def minimax_alpha_beta(graf, trouglovi, trouglovi1, trouglovi2, moguci_potezi, odigrani, dubina, moj_potez, trenutni_igrac, alpha=(None, -150), beta=(None, 150)):
    if moj_potez:
        return max_value(graf, trouglovi, trouglovi1, trouglovi2, moguci_potezi, odigrani, dubina, alpha, beta, trenutni_igrac)
    else:
        return min_value(graf, trouglovi, trouglovi1, trouglovi2, moguci_potezi, odigrani, dubina, alpha, beta, trenutni_igrac)


def pokreni_igru():
  global trenutni_igrac
  global broj_prve_grupe
  global potezi_simulirano
  while True:
    unos = input("Unesite broj čvorova u prvoj grupi (4-8): ")
    if unos.isdigit():
      broj_prve_grupe = int(unos)
      if 4 <= broj_prve_grupe <= 8:
        break
      else:
        print("Broj mora biti između 4 i 8! Pokušajte ponovo.")
    else:
      print("Molimo unesite validan broj, a ne karaktere.")

  graf = formiraj_graf(broj_prve_grupe)
  izracunaj_moguce_poteze(graf)


  while True:
    unos2 = input("Ko počinje? (0 za Igrač 1, 1 za Igrač 2): ")
    if unos2.isdigit():
      trenutni_igrac = int(unos2)
      if 0 <= trenutni_igrac <= 1:
        break
      else:
        print("Unesite odgovarajuće brojeve!")
    else:
      print("Molimo unesite validan broj, a ne karaktere.")

  potez = True if igraci[trenutni_igrac] == "Igrač 2" else False

  while not zavrsi_igru():
    print("\n")
    prikazi_graf_tufne(graf)
    print(f"\nNa potezu je: {igraci[trenutni_igrac]}")
    #print(potezi)
    if igraci[trenutni_igrac]== "Igrač 1":
      pocetni_cvor = input("Unesite početni čvor: ")
      smer = input("Unesite smer (DD, DL, D): ")

      try:
        uspesan = dodaj_potez_duzine_4(graf, pocetni_cvor, smer, odigrani_potezi, potezi, trouglici, trouglici1, trouglici2, trenutni_igrac)

        if uspesan:
          potez = not potez
          # Izmeni igrača nakon poteza
          trenutni_igrac = (trenutni_igrac + 1) % len(igraci)
      except ValueError as e:
        print(f"Greška: {e}")

    elif igraci[trenutni_igrac]== "Igrač 2":
      min_max_alpha_beta_result = minimax_alpha_beta(graf, trouglici, trouglici1, trouglici2, potezi, odigrani_potezi, 3, potez, trenutni_igrac, alpha=(None, -150), beta=(None, 150))
      print(f"Min-Max α-β: {min_max_alpha_beta_result}")
      naj = min_max_alpha_beta_result[0]
      cvor, pravac = naj
      try:
        dodaj_potez_duzine_4(graf, cvor, pravac, odigrani_potezi, potezi, trouglici, trouglici1, trouglici2, trenutni_igrac)

        trenutni_igrac = (trenutni_igrac + 1) % len(igraci)
        potez = not potez
        print(f"odigran potez {cvor}, {pravac}")
      except ValueError as e:
        print(f"Greška: {e}")


  print("\n")
  prikazi_graf_tufne(graf)
  print("\n")
  print("KRAJ!")
  if(len(trouglici1)>len(trouglici2)):
    print(f"Pobednik je igrač 1! Formirao je {len(trouglici1)} trouglova!")
  elif (len(trouglici2) > len(trouglici1)):
    print(f"Pobednik je igrač 2! Formirao je {len(trouglici2)} trouglova! ")
  else:
    print("Nerešeno!")


pokreni_igru()