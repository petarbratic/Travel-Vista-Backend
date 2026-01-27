using Explorer.Stakeholders.API.Dtos;
using Explorer.Stakeholders.API.Public;
using Explorer.Stakeholders.Core.Domain;
using Explorer.Stakeholders.Core.Domain.RepositoryInterfaces;

namespace Explorer.Stakeholders.Core.UseCases;

public class RankRewardService : IRankRewardService
{
    private readonly ITouristRepository _touristRepository;
    private readonly ITouristRankRewardsRepository _rankRewardsRepository;
    private readonly IWalletRepository _walletRepository;
    private readonly IWalletTransactionRepository _txRepo;


    public RankRewardService(
        ITouristRepository touristRepository,
        ITouristRankRewardsRepository rankRewardsRepository,
        IWalletRepository walletRepository,
        IWalletTransactionRepository txRepo)
    {
        _touristRepository = touristRepository;
        _rankRewardsRepository = rankRewardsRepository;
        _walletRepository = walletRepository;
        _txRepo = txRepo;
    }

    public RankRewardClaimResultDto ClaimRankRewards(long touristId)
    {
        var tourist = _touristRepository.Get(touristId);
        if (tourist == null)
            throw new KeyNotFoundException($"Tourist with id {touristId} not found.");

        // Get or create rank rewards tracking
        var rankRewards = _rankRewardsRepository.GetByTouristId(touristId);
        if (rankRewards == null)
        {
            rankRewards = new TouristRankRewards(touristId);
            rankRewards = _rankRewardsRepository.Create(rankRewards);
        }

        var claimedRanks = new List<string>();
        int totalAcAwarded = 0;

        // Check Gold (Level 10+)
        if (tourist.Level >= 10 && !rankRewards.GoldRewardClaimed)
        {
            rankRewards.ClaimGoldReward();
            totalAcAwarded += 500;
            claimedRanks.Add("Gold");
        }

        // Check Platinum (Level 15+)
        if (tourist.Level >= 15 && !rankRewards.PlatinumRewardClaimed)
        {
            rankRewards.ClaimPlatinumReward();
            totalAcAwarded += 500;
            claimedRanks.Add("Platinum");
        }

        // Check Diamond (Level 20+)
        if (tourist.Level >= 20 && !rankRewards.DiamondRewardClaimed)
        {
            rankRewards.ClaimDiamondReward();
            totalAcAwarded += 1000;
            claimedRanks.Add("Diamond");
        }

        // Check Vista (Level 30+)
        if (tourist.Level >= 30 && !rankRewards.VistaRewardClaimed)
        {
            rankRewards.ClaimVistaReward();
            totalAcAwarded += 2000;
            claimedRanks.Add("Vista");
        }

        if (claimedRanks.Count == 0)
        {
            return new RankRewardClaimResultDto
            {
                Success = false,
                Message = "No unclaimed rank rewards available.",
                AcAwarded = 0,
                ClaimedRanks = new List<string>()
            };
        }

        // Update rank rewards tracking
        _rankRewardsRepository.Update(rankRewards);

        // Add AC to wallet
        var wallet = _walletRepository.GetByPersonId(touristId);
        if (wallet == null)
            throw new InvalidOperationException($"Wallet not found for tourist {touristId}");

        wallet.AddAc(totalAcAwarded);
        _walletRepository.Update(wallet);
        _txRepo.Create(new WalletTransaction(
            personId: touristId,
            amountAc: totalAcAwarded,
            type: WalletTransactionType.RankRewardAc,
            description: $"Rank reward: +{totalAcAwarded} AC ({string.Join(", ", claimedRanks)})",
            referenceType: "RankReward",
            referenceId: rankRewards.Id
        ));

        return new RankRewardClaimResultDto
        {
            Success = true,
            Message = $"Claimed {string.Join(", ", claimedRanks)} rank rewards!",
            AcAwarded = totalAcAwarded,
            ClaimedRanks = claimedRanks
        };
    }
}